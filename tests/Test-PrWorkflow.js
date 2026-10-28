// Execute only with mocks: no GitHub request or comment is sent.
const fs = require('fs');
const assert = require('assert');
const yaml = fs.readFileSync('.github/workflows/ghi-chu-pr-tieng-viet.yml', 'utf8');
assert(yaml.includes('pull_request:') && !yaml.includes('pull_request_target:'));
assert(yaml.includes('head.repo.full_name == github.repository'));
assert(yaml.includes('permissions: {}') && yaml.includes('timeout-minutes: 5'));
assert(yaml.includes('actions/github-script@f28e40c7f34bde8b3046d885e986cb6290c5673b'));
assert(!yaml.includes('actions/checkout'));
const source = yaml.split('script: |')[1].split(/\r?\n/).filter(line=>line.trim()).map(line=>line.slice(12)).join('\n');
const AsyncFunction = Object.getPrototypeOf(async function(){}).constructor;
let comments = [];
const github = {rest:{repos:{getCommit:async()=>({data:{commit:{message:'Ghi chú @someone\r\nKhông đưa phần body này vào ghi chú'},html_url:'https://github.com/example/repo/commit/mock'}})},
  issues:{createComment:async request=>comments.push(request)}}};
const context={repo:{owner:'example',repo:'repo'},payload:{pull_request:{number:7,head:{ref:'codex/test',sha:'a'.repeat(40)}}}};
(async()=>{
  await new AsyncFunction('github','context',source)(github,context);
  assert.equal(comments.length,1);
  const body=comments[0].body;
  assert(body.includes('\n') && !body.includes('\\n'));
  assert(!body.includes('Không đưa phần body'));
  assert(!body.includes('@someone'));
  assert.equal(comments[0].issue_number,7);
  console.log('PASS PR workflow 10 assertions (mock only; GitHub run NOT RUN)');
})().catch(()=>{console.error('FAIL PR workflow mock');process.exit(1);});
