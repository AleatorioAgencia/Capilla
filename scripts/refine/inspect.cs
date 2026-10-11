var all = new FilteredElementCollector(document).WhereElementIsNotElementType().ToElements();
var elements = all.Where(e => e is Wall || e is Floor || e is RoofBase || e is Toposolid || e is FamilyInstance || e is DirectShape || e is Autodesk.Revit.DB.Architecture.Railing).Select(e => {
  var b=e.get_BoundingBox(null); var lp=e.Location as LocationPoint;
  return new { id=e.Id.Value, name=e.Name, category=e.Category == null ? "" : e.Category.Name, kind=e.GetType().Name, type=e.GetTypeId().Value,
    min=b==null?null:new[]{b.Min.X*.3048,b.Min.Y*.3048,b.Min.Z*.3048}, max=b==null?null:new[]{b.Max.X*.3048,b.Max.Y*.3048,b.Max.Z*.3048},
    point=lp==null?null:new[]{lp.Point.X*.3048,lp.Point.Y*.3048,lp.Point.Z*.3048} };
}).ToList();
var result=new {title=document.Title,path=document.PathName,modified=document.IsModified,elements,
  materials=new FilteredElementCollector(document).OfClass(typeof(Material)).Cast<Material>().Where(m=>m.Name.StartsWith("Finca")).Select(m=>new{id=m.Id.Value,name=m.Name,color=new[]{m.Color.Red,m.Color.Green,m.Color.Blue}}),
  views=new FilteredElementCollector(document).OfClass(typeof(View3D)).Cast<View3D>().Where(v=>!v.IsTemplate).Select(v=>new{id=v.Id.Value,name=v.Name})};
var json=Newtonsoft.Json.JsonConvert.SerializeObject(result);
System.IO.File.WriteAllText(@"C:\Users\admin\.codex\.chatgpt-projects\g-p-6acadee8e81c8191910d9dd6cf16b614\Capilla\scripts\refine\before.json",json);
return new {title=document.Title,path=document.PathName,count=elements.Count,plants=elements.Count(e=>e.category=="Plantas"),topos=elements.Count(e=>e.kind=="Toposolid")};
