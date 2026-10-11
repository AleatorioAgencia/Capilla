const { run } = require('./revit.cjs');
const path = require('node:path');
const fs = require('node:fs');
const source = path.join(__dirname, 'NewTakesModel.cs');
const action = process.argv[2] || 'inspect';
const code = `
try {
var source=System.IO.File.ReadAllText(@"${source}");
var tree=Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source);
var refs=AppDomain.CurrentDomain.GetAssemblies().Where(a=>!a.IsDynamic&&!string.IsNullOrEmpty(a.Location)).GroupBy(a=>a.GetName().Name,StringComparer.OrdinalIgnoreCase).Select(g=>Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(g.First().Location)).Cast<Microsoft.CodeAnalysis.MetadataReference>().ToList();
var comp=Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("CapillaRefine_"+Guid.NewGuid().ToString("N"),new[]{tree},refs,new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));
using(var ms=new System.IO.MemoryStream()) {
var result=comp.Emit(ms); if(!result.Success) return new {error=string.Join("\\n",result.Diagnostics.Where(d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Select(d=>d.ToString()))};
var asm=System.Reflection.Assembly.Load(ms.ToArray());
return asm.GetType("CapillaRefine.Model").GetMethod("Execute").Invoke(null,new object[]{document,uiapp,"${action.replace(/[^a-zA-Z0-9_-]/g,'')}"});
}
} catch(Exception e) {return new {error=e.ToString()};}`;
run(code, 'none').then(result => {
  fs.writeFileSync(path.join(__dirname, `new-${action}.json`),JSON.stringify(result,null,2));
  console.log(JSON.stringify(result,null,2));
  if(result?.error) process.exitCode=1;
},e=>{console.error(e);process.exitCode=1;});


