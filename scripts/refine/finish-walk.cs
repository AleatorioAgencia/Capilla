if(!document.PathName.EndsWith("Capilla_recorrido_9399.rvt"))throw new Exception("Documento inesperado");
var v=new FilteredElementCollector(document).OfClass(typeof(View3D)).Cast<View3D>().First(x=>x.Name=="CAPILLA - 07 Corredor recorrido");
uiapp.ActiveUIDocument.ActiveView=v;
document.Save();
return new{path=document.PathName,saved=!document.IsModified,warnings=document.GetWarnings().Count,view=v.Name};
