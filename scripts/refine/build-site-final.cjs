const fs=require('node:fs'),path=require('node:path');
const target=path.join(__dirname,'SiteModel.cs');
let code=fs.readFileSync(target,'utf8');
if(!code.includes('static void SitePortal()'))code=code.replace(' public static object Execute(',fs.readFileSync(path.join(__dirname,'PortalStage.cs'),'utf8')+'\n public static object Execute(');
code=code.replaceAll('n.StartsWith("Gallinero")?1:0','(n.StartsWith("Gallinero")||n.StartsWith("Portada"))?1:0');
if(!code.includes('if(action=="siteportal")'))code=code.replace('if(action=="siteshed")','if(action=="siteportal")SitePortal();else if(action=="siteshed")');
fs.writeFileSync(target,code);
