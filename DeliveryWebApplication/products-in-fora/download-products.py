from selenium import webdriver
from json import dump

urls=open('sitemap-products.txt').read().strip().split('\n')

driver = webdriver.Chrome('operadriver.exe')
driver.implicitly_wait(5)

ans={}

def TRY(func):
    try:
        return func()
    except:
        return ''

def parse(url):
    while True:
        try:
            driver.get(url)
            break
        except Exception as e:
            print(repr(e))
    title=TRY(lambda:driver.find_element_by_class_name('title').text.strip())
    weight=TRY(lambda:driver.find_element_by_class_name('product-weight').text.strip())
    price=TRY(lambda:driver.find_element_by_class_name('current-price').text.strip().replace('\n','.'))
    details=TRY(lambda:{el.text.strip().split('\n',1)[0].strip():el.text.strip().split('\n',1)[1].strip() for el in driver.find_elements_by_class_name('product-details-column')})
    country=TRY(lambda:details['Країна'])
    trademark=TRY(lambda:details['Торгова марка'])
    category=TRY(lambda:driver.find_elements_by_class_name('bread-crumbs-link')[1].text.strip())
    del url,details
    return locals()

i=0
for url in urls:
    ans[url]=parse(url)
    i+=1
    if i%10==0:
        print(i,'/',len(urls))

dump(ans,open('products.json','w'),ensure_ascii=False,indent=4)
