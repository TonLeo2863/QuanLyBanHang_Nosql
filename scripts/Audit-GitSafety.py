"""Read-only Git audit. Never print source lines, secret values, or credential hashes.

History findings block publication, not a separate safe local documentation commit.
This is a pattern/exact-match audit, not proof that every possible secret is absent.
"""
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import urllib.parse

def git(*args):
    result=subprocess.run(['git',*args],capture_output=True)
    if result.returncode: raise RuntimeError('Git audit command failed')
    return result.stdout

PATTERNS={
    'github-token':rb'gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,}',
    'aws-access-key':rb'AKIA[0-9A-Z]{16}',
    'private-key':rb'-----BEGIN [A-Z ]*PRIVATE KEY-----',
    'credential-in-url':rb'https?://[^\s/\x22\x27]+:[^\s/\x22\x27]+@',
    'literal-password':rb'(?i)\b(?:password|passwd|pwd|client_secret|api_key|access_token|secret_access_key)[\x22\x27]?\s*[:=]\s*[\x22\x27]([^\x22\x27\r\n]{4,})[\x22\x27]',
}

def inspect(data,path,secret=None):
    rules=[];matches_configured_credential=False
    for name,pattern in PATTERNS.items():
        for match in re.finditer(pattern,data):
            if name=='literal-password':
                if secret and match.group(1)==secret: matches_configured_credential=True
                if path=='tests/Test-Backup.py' and match.group(1)==b'fake-forbidden' and (not secret or secret not in match.group(1)):
                    rules.append('reviewed-negative-test-fixture');continue
            rules.append(name)
    # Identify literal Basic credentials without ever persisting their values.
    for match in re.finditer(rb'GetBytes\s*\(\s*\x22([^\x22\r\n]+:[^\x22\r\n]+)\x22\s*\)',data):
        payload=match.group(1)
        if payload.startswith((b'http:',b'https:')): continue
        rules.append('literal-basic-credential')
        if secret and payload.split(b':',1)[1]==secret: matches_configured_credential=True
    for match in re.finditer(rb'NetworkCredential\s*\(\s*\x22[^\x22]*\x22\s*,\s*\x22([^\x22]+)\x22',data):
        rules.append('literal-network-credential')
        if secret and match.group(1)==secret: matches_configured_credential=True
    if secret and secret in data: rules.append('configured-secret-match')
    # Keep metadata only. A pattern finding can be a fixture; triage it privately.
    return sorted(set(rules)),matches_configured_credential

def remote_check(url):
    expected='TonLeo2863/QuanLyBanHang_Nosql'
    if url.startswith('git@github.com:'):
        return url.split(':',1)[1].removesuffix('.git')==expected,False
    parsed=urllib.parse.urlsplit(url)
    embedded=bool(parsed.password or (parsed.scheme in ['http','https'] and parsed.username))
    valid=(parsed.scheme in ['https','ssh'] and parsed.hostname=='github.com' and
           parsed.path.strip('/').removesuffix('.git')==expected and not parsed.query and not parsed.fragment and not embedded)
    return valid,embedded

def audit():
    password=os.environ.get('COUCHDB_PASSWORD')
    secret=password.encode('utf-8') if password else None
    report=dict(branch=git('branch','--show-current').decode().strip(),head=git('rev-parse','HEAD').decode().strip(),
                configured_secret_available=bool(secret),worktree_findings=[],history_findings=[],reviewed_fixtures=[])
    for path in filter(None,git('ls-files','--cached','--others','--exclude-standard','-z').decode().split('\0')):
        file=Path(path)
        if not file.is_file(): continue
        rules,_=inspect(file.read_bytes(),path,secret)
        if 'reviewed-negative-test-fixture' in rules:
            report['reviewed_fixtures'].append(dict(path=path,rule='negative-test-fixture'));rules.remove('reviewed-negative-test-fixture')
        if rules: report['worktree_findings'].append(dict(path=path,rules=rules))
    commits=git('rev-list','--all','--reflog').decode().splitlines()
    report['commits_scanned']=len(commits)
    for sha in commits:
        rules,_=inspect(git('show','-s','--format=%B',sha),'commit-message',secret)
        if rules: report['history_findings'].append(dict(commit=sha[:12],path='commit-message',rules=rules))
    objects=git('rev-list','--objects','--all','--reflog').decode().splitlines()
    process=subprocess.Popen(['git','cat-file','--batch'],stdin=subprocess.PIPE,stdout=subprocess.PIPE,stderr=subprocess.DEVNULL)
    blob_count=0;credential_match=False
    try:
        for entry in objects:
            oid,_,path=entry.partition(' ')
            process.stdin.write((oid+'\n').encode());process.stdin.flush()
            header=process.stdout.readline().decode().split()
            if len(header)!=3: raise RuntimeError('Git object read failed')
            size=int(header[2]);data=process.stdout.read(size);process.stdout.read(1)
            if len(data)!=size: raise RuntimeError('Git object truncated')
            if header[1]!='blob': continue
            blob_count+=1
            rules,matched=inspect(data,path,secret);credential_match|=matched
            if 'reviewed-negative-test-fixture' in rules: rules.remove('reviewed-negative-test-fixture')
            if rules: report['history_findings'].append(dict(blob=oid[:12],path=path,rules=rules))
    finally:
        process.stdin.close();process.stdout.close();process.wait()
    report['blobs_scanned']=blob_count
    report['configured_password_matches_historical_literal_credential']=credential_match
    report['origin']={}
    for mode,args in [('fetch',('remote','get-url','--all','origin')),('push',('remote','get-url','--push','--all','origin'))]:
        urls=git(*args).decode().splitlines();checks=[remote_check(u) for u in urls]
        report['origin'][mode]=dict(count=len(urls),target_match=bool(checks) and all(v[0] for v in checks),embedded_credential=any(v[1] for v in checks))
    report['limitations']='Local refs and reflogs only; not un-fetched remote refs, forks, caches, attachments or a forensic proof. No old credential authentication attempted.'
    print(json.dumps(report,ensure_ascii=True,indent=2))
    unsafe_remote=any(not value['target_match'] or value['embedded_credential'] for value in report['origin'].values())
    return 1 if report['worktree_findings'] or report['history_findings'] or unsafe_remote else (0 if secret else 2)

if __name__=='__main__':
    try: sys.exit(audit())
    except Exception as e:
        print('FAIL Git safety audit: '+type(e).__name__);sys.exit(2)
