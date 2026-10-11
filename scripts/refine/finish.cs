if(!document.PathName.EndsWith("Capilla_detallada.rvt",StringComparison.OrdinalIgnoreCase))throw new Exception("Unexpected active model");
var v=new FilteredElementCollector(document).OfClass(typeof(View3D)).Cast<View3D>().First(x=>x.Name=="CAPILLA - 05 Dron");
uiapp.ActiveUIDocument.ActiveView=v;
uiapp.ActiveUIDocument.RefreshActiveView();
document.Save();
return new{path=document.PathName,view=v.Name,saved=!document.IsModified,warnings=document.GetWarnings().Count};
