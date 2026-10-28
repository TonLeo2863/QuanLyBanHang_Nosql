"""JSON codec preserving decimal numbers (no binary-float round trip)."""
from decimal import Decimal
import json
import math

def _pairs(pairs):
    result={}
    for key,value in pairs:
        if key in result: raise ValueError('JSON chứa khóa trùng')
        result[key]=value
    return result

def loads(data):
    def invalid(_): raise ValueError('JSON có số không hữu hạn')
    return json.loads(data,parse_float=Decimal,object_pairs_hook=_pairs,parse_constant=invalid)

def dumps(value):
    if value is None: return 'null'
    if value is True: return 'true'
    if value is False: return 'false'
    if isinstance(value,str): return json.dumps(value,ensure_ascii=False)
    if isinstance(value,int): return str(value)
    if isinstance(value,Decimal):
        if not value.is_finite(): raise ValueError('Số không hữu hạn')
        text=format(value,'f')
        if value==0: return '0'
        return text.rstrip('0').rstrip('.') if '.' in text else text
    if isinstance(value,float):
        if not math.isfinite(value): raise ValueError('Số không hữu hạn')
        return json.dumps(value,allow_nan=False)
    if isinstance(value,list): return '['+','.join(dumps(x) for x in value)+']'
    if isinstance(value,dict):
        if not all(isinstance(k,str) for k in value): raise ValueError('Khóa JSON phải là chuỗi')
        return '{'+','.join(json.dumps(k,ensure_ascii=False)+':'+dumps(value[k]) for k in sorted(value))+'}'
    raise ValueError('Kiểu JSON không được hỗ trợ')
