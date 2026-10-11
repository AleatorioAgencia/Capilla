var root=@"C:\Users\admin\.codex\.chatgpt-projects\g-p-6acadee8e81c8191910d9dd6cf16b614\Capilla";
var dest=System.IO.Path.Combine(root,"revit","Capilla_detallada.rvt");
if(document.PathName!=dest) {
 if(System.IO.File.Exists(dest)) throw new Exception("La copia detallada ya existe; revisar antes de continuar.");
 document.SaveAs(dest,new SaveAsOptions{Compact=true,MaximumBackups=3});
}
View3D v;
using(var t=new Transaction(document,"Capilla - vistas de comprobacion")) {
 t.Start();
 var vt=new FilteredElementCollector(document).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().First(x=>x.ViewFamily==ViewFamily.ThreeDimensional);
 v=View3D.CreateIsometric(document,vt.Id); v.Name="CAPILLA - 01 Conjunto"; v.DetailLevel=ViewDetailLevel.Fine; v.DisplayStyle=DisplayStyle.Shading;
 double u=1/.3048; var eye=new XYZ(-65*u,-150*u,100*u); var target=new XYZ(0,-5*u,0); var f=(target-eye).Normalize();var r=f.CrossProduct(XYZ.BasisZ).Normalize();
 v.SetOrientation(new ViewOrientation3D(eye,r.CrossProduct(f),f));
 v.SetSectionBox(new BoundingBoxXYZ{Min=new XYZ(-68*u,-75*u,-15*u),Max=new XYZ(75*u,65*u,40*u)});v.IsSectionBoxActive=true;
 foreach(var c in new[]{BuiltInCategory.OST_Levels,BuiltInCategory.OST_Grids,BuiltInCategory.OST_Sections,BuiltInCategory.OST_Elev,BuiltInCategory.OST_SectionBox}) try{v.SetCategoryHidden(new ElementId(c),true);}catch{}
 t.Commit();
}
uiapp.ActiveUIDocument.ActiveView=v;
var opts=new ImageExportOptions{FilePath=System.IO.Path.Combine(root,"renders","antes"),ExportRange=ExportRange.SetOfViews,HLRandWFViewsFileType=ImageFileType.PNG,ShadowViewsFileType=ImageFileType.PNG,ImageResolution=ImageResolution.DPI_150,ZoomType=ZoomFitType.FitToPage,PixelSize=1800};
opts.SetViewsAndSheets(new List<ElementId>{v.Id});document.ExportImage(opts);document.Save();
return new {path=document.PathName,view=v.Id.Value};
