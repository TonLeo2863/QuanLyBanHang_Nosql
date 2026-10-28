"""Create-only app account. Password prompted privately; never a default password."""
import base64
import getpass
import hashlib
import re
import secrets
import sys
from couch import Couch

def user_document(name, password, role):
    if not re.fullmatch('[a-z0-9][a-z0-9._-]{2,49}',name) or role not in ['Administrator','OrderManager','Shipper']:
        raise ValueError('Tài khoản/vai trò không hợp lệ')
    if not 12<=len(password)<=128: raise ValueError('Mật khẩu phải có 12–128 ký tự')
    salt=secrets.token_bytes(16)
    return dict(_id='app-user-'+name,type='app_user',username=name,role=role,enabled=True,iterations=600000,
                salt=base64.b64encode(salt).decode(),passwordHash=base64.b64encode(hashlib.pbkdf2_hmac('sha256',password.encode(),salt,600000,32)).decode())

if __name__=='__main__':
    try:
        name=input('Tên tài khoản ứng dụng: ').strip()
        role=input('Vai trò (Administrator/OrderManager/Shipper): ').strip()
        password=getpass.getpass('Mật khẩu riêng (12–128 ký tự): ')
        if password != getpass.getpass('Nhập lại mật khẩu: '): raise ValueError('Mật khẩu xác nhận không khớp')
        if not Couch().create(user_document(name,password,role)): raise ValueError('Mã đã tồn tại; không ghi đè')
        print('PASS tạo tài khoản ứng dụng; không thay tài khoản CouchDB')
    except Exception as e:
        print('FAIL tạo tài khoản: '+type(e).__name__); sys.exit(1)
