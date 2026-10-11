const fs=require('node:fs'),path=require('node:path');
const file=path.join(__dirname,'build-review.cjs');let code=fs.readFileSync(file,'utf8');
code=code.replace('Documentada. La portada del modelo todavía requiere una revisión de detalle.','Sustituido el volumen cerrado por pilares de ladrillo con juntas, muros laterales escalonados y cubierta de teja sobre el paso. Portón lateral aún pendiente.');
code=code.replace('Interiores completos y detalles de portada pendientes.','Interiores completos y portón lateral de acceso pendientes.');
code=code.replace('const review={','findings.sort((a,b)=>a.at-b.at);\nconst review={');
code=code.replace("'10 Lago y kiosco inferior'].map","'10 Lago y kiosco inferior','11 Portada de acceso'].map");
code=code.replace('Selecciona un tiempo a la derecha','Selecciona un tiempo de la lista');
code=code.replace('b.onclick=()=>seek(nearest(Number(b.dataset.time)))','b.onclick=()=>{seek(nearest(Number(b.dataset.time)));document.querySelector("aside").scrollIntoView({block:"start",behavior:"smooth"});}');
fs.writeFileSync(file,code);
