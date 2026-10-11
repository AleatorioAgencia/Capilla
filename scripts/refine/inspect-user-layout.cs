var elements=new List<object>();
foreach(var e in new FilteredElementCollector(document).WhereElementIsNotElementType()){
 var ds=e as DirectShape;
 if(!(e is Wall||e is Floor||e is RoofBase||e is Toposolid||ds!=null))continue;
 if(ds!=null&&ds.Category.Id.Value==(long)BuiltInCategory.OST_Planting)continue;
 var b=e.get_BoundingBox(null);if(b==null)continue;
 var c=(b.Min+b.Max)*.1524;
 var floor=e as Floor;
 elements.Add(new{id=e.Id.Value,type=e.GetType().Name,name=e.Name,tag=ds?.ApplicationDataId,comment=e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString(),center=new[]{c.X,c.Y,c.Z},min=new[]{b.Min.X*.3048,b.Min.Y*.3048,b.Min.Z*.3048},max=new[]{b.Max.X*.3048,b.Max.Y*.3048,b.Max.Z*.3048},vertices=floor?.GetSlabShapeEditor()?.SlabShapeVertices.Cast<SlabShapeVertex>().Select(v=>new[]{v.Position.X*.3048,v.Position.Y*.3048,v.Position.Z*.3048}).ToList()});
}
return new{path=document.PathName,title=document.Title,modified=document.IsModified,readOnly=document.IsReadOnly,activeView=uiapp.ActiveUIDocument.ActiveView.Name,elements=elements};
