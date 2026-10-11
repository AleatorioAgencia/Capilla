if(!document.PathName.EndsWith("Capilla_ajuste_nuevas_tomas.rvt"))throw new Exception("Documento inesperado");
var original=System.IO.Path.Combine(System.IO.Path.GetDirectoryName(document.PathName),"Capilla_conjunto_corregido.rvt");
var baseline=uiapp.Application.OpenDocumentFile(original);
try{
 var rows=new List<object>();
 foreach(var e in new FilteredElementCollector(document).WhereElementIsNotElementType()){
  if(!(e is DirectShape||e is Floor||e is Wall||e is RoofBase))continue;
  var old=baseline.GetElement(e.UniqueId);if(old==null)continue;
  var b=e.get_BoundingBox(null);var a=old.get_BoundingBox(null);if(a==null||b==null)continue;
  var delta=(b.Min+b.Max-a.Min-a.Max)*.1524;
  if(delta.GetLength()<.001)continue;
  var oc=(a.Min+a.Max)*.1524;var nc=(b.Min+b.Max)*.1524;
  rows.Add(new{id=e.Id.Value,type=e.GetType().Name,tag=(e as DirectShape)?.ApplicationDataId,oldCenter=new[]{oc.X,oc.Y,oc.Z},center=new[]{nc.X,nc.Y,nc.Z},delta=new[]{delta.X,delta.Y,delta.Z},oldMinZ=a.Min.Z*.3048,newMinZ=b.Min.Z*.3048});
 }
 return rows;
}finally{baseline.Close(false);}
