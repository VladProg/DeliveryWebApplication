from json import load,dump
from decimal import Decimal as decimal

def dec(s,e=''):
    if s.endswith('/у'):
        s=s[:-2]
    if s.endswith('/уп'):
        s=s[:-3]
    if not s.endswith(e):
        return None
    s=s[:len(s)-len(e)].replace(',','.')
    try:
        if '*' in s:
            a,b=s.split('*')
            return decimal(a)*decimal(b)
        else:
            return decimal(s)
    except:
        return None

before=load(open('products.json'))
after=[]

for url in before:
    cur=before[url]
    if not cur['title']:
        continue
    if cur['country']=='Росія':
        continue
    if cur['trademark'] in ['','Без ТМ']:
        cur['trademark']='—'
    if cur['country']=='':
        cur['country']='—'
    w=cur['weight']
    if (x:=dec(w,'кг')) is not None: w=x
    elif (x:=dec(w,'г')) is not None: w=x/1000
    elif (x:=dec(w,'л')) is not None: w=x
    elif (x:=dec(w,'мл')) is not None: w=x/1000
    elif w=='кг': w=''
    elif 'шт' in w: continue
    elif w=='бух': continue
    elif w=='пачка': continue
    elif w=='уп': continue
    elif w=='пара': continue
    elif w=='1 пара': continue
    elif w=='пучок': continue
    elif w=='10м': continue
    elif w=='5м': continue
    else: print(w,url,sep='\t')
    cur['weight']=str(w)
    after.append(cur)
    
dump(after,open('products-processed.json','w'),ensure_ascii=True,indent=4)
print(len(before),'->',len(after))
