if(!document.PathName.EndsWith("Capilla_conjunto_corregido.rvt"))throw new Exception("Documento inesperado");
var portal=new FilteredElementCollector(document).OfClass(typeof(View3D)).Cast<View3D>().First(x=>x.Name=="CAPILLA - 11 Portada de acceso");
using(var t=new Transaction(document,"Capilla - presentacion portada")){
 t.Start();var box=portal.GetSectionBox();var ids=new List<ElementId>();
 foreach(var p in new FilteredElementCollector(document).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(p=>p.Category.Id.Value==(long)BuiltInCategory.OST_Planting)){
  var b=p.get_BoundingBox(null);if(b==null)continue;var c=(b.Min+b.Max)/2;if(c.X<box.Min.X||c.X>box.Max.X||c.Y<box.Min.Y||c.Y>box.Max.Y)if(p.CanBeHidden(portal))ids.Add(p.Id);
 }
 if(ids.Count>0)portal.HideElements(ids);
 foreach(Category cat in document.Settings.Categories){foreach(Category sub in cat.SubCategories)if(sub.Name.IndexOf("curva",StringComparison.OrdinalIgnoreCase)>=0||sub.Name.IndexOf("contour",StringComparison.OrdinalIgnoreCase)>=0)try{portal.SetCategoryHidden(sub.Id,true);}catch{}}
 t.Commit();
}
var v=new FilteredElementCollector(document).OfClass(typeof(View3D)).Cast<View3D>().First(x=>x.Name=="CAPILLA - 05 Dron");
uiapp.ActiveUIDocument.ActiveView=v;
document.Save();
return new{path=document.PathName,saved=!document.IsModified,warnings=document.GetWarnings().Select(w=>new{message=w.GetDescriptionText(),elements=w.GetFailingElements().Select(id=>id.Value).ToList()}).ToList(),view=v.Name};
