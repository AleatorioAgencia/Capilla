if(!document.PathName.EndsWith("Capilla_ajuste_nuevas_tomas.rvt"))throw new Exception("Documento inesperado");
var changes=new List<object>();
using(var t=new Transaction(document,"Capilla - encuadres de detalle sin fragmentos cortados")){
 t.Start();
 var groups=new FilteredElementCollector(document).OfClass(typeof(Group)).Cast<Group>().ToList();
 foreach(var spec in new[]{new[]{"CAPILLA - 13 Caballerizas y cocina","CAPILLA - Lago 1","CAPILLA - Lago 2"},new[]{"CAPILLA - 15 Salida trasera hacia lagos","CAPILLA - Caballerizas","CAPILLA - Cocina"}}){
  var view=new FilteredElementCollector(document).OfClass(typeof(View3D)).Cast<View3D>().First(v=>v.Name==spec[0]);
  var ids=new List<ElementId>();
  foreach(var g in groups.Where(g=>spec.Skip(1).Contains(g.GroupType.Name))){
   if(g.CanBeHidden(view)&&!g.IsHidden(view))ids.Add(g.Id);
   else ids.AddRange(g.GetMemberIds().Where(id=>{var e=document.GetElement(id);return e!=null&&e.CanBeHidden(view)&&!e.IsHidden(view);}));
  }
  if(ids.Count>0)view.HideElements(ids.Distinct().ToList());
  changes.Add(new{view=view.Name,hidden=ids.Select(id=>id.Value).ToList()});
 }
 t.Commit();
}
document.Save();return new{saved=!document.IsModified,changes};
