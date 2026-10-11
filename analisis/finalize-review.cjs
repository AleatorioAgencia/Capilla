const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'..');
const read=p=>JSON.parse(fs.readFileSync(path.join(root,p),'utf8').replace(/^\uFEFF/,''));
const review=read('analisis/revision_completa.json');
const final=read('scripts/refine/site-final.json');
if(!final.saved||!final.path.endsWith('Capilla_conjunto_corregido.rvt'))throw new Error('Guardado sin confirmar');
const html=fs.readFileSync(path.join(__dirname,'revision_completa.html'),'utf8');
const refs=[...html.matchAll(/(?:src|href)="([^"]+)"/g)].map(m=>m[1]);
const missing=refs.filter(p=>!fs.existsSync(path.resolve(__dirname,decodeURIComponent(p))));
if(missing.length)throw new Error('Referencias inexistentes: '+missing.join(', '));
const model=path.join(root,'revit/Capilla_conjunto_corregido.rvt');
const manifest={
 revision:'Corrección del conjunto a partir del recorrido IMG_9399',
 savedAt:fs.statSync(model).mtime.toISOString(),model:'Capilla_conjunto_corregido.rvt',
 baseline:'Capilla_recorrido_9399.rvt',modelBytes:fs.statSync(model).size,
 modelSha256:crypto.createHash('sha256').update(fs.readFileSync(model)).digest('hex'),
 referenceVideo:review.video,referenceSha256:'2845F428A41D5D5055EC9748DA75D4FA638348ADDEF909406041D601B6FB8E5F',
 analysis:'../analisis/revision_completa.json',interactiveReview:'../analisis/revision_completa.html',
 reviewMethod:review.visualReview,sourceFrameCount:19942,proxyFrameCount:19942,
 confirmedByOwner:review.userClarifications,
 changes:review.findings.map(f=>({element:f.name,timeSeconds:f.at,change:f.change})),
 lakeCount:3,lakeDistribution:{upper:2,lowerNextToKiosk:1},
 activeView:final.view,verifiedViews:['08 Tanques gallinero cabana','09 Lagos superiores y caseta','10 Lago y kiosco inferior','11 Portada de acceso'],
 warningCount:final.warnings.length,warnings:final.warnings,
 validation:{modelSaved:true,referenceLinksChecked:refs.length,proxyDurationSeconds:665.2646,videoMarkerSeekVerified:true,nextFrameButtonVerified:true},
 limitations:[...review.limitations,'Vegetación y mobiliario esquemáticos; no se considera terminada una reproducción de fidelidad ultra.','Siete avisos de Revit: tres de espesor de pavimento, dos de solape entre suelos y dos entre suelo y terreno.','Los detalles de malla, tanques, kiosco y caseta incluyen DirectShape; no todos son familias paramétricas.']
};
fs.writeFileSync(path.join(root,'revit/Capilla_conjunto_corregido.manifest.json'),JSON.stringify(manifest,null,2));
console.log(JSON.stringify({saved:final.saved,modelBytes:manifest.modelBytes,linksChecked:refs.length,missing,warnings:manifest.warningCount,lakes:manifest.lakeCount}));
