using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;

namespace CapillaRefine {
public class Warnings : IFailuresPreprocessor {
 public FailureProcessingResult PreprocessFailures(FailuresAccessor a) { foreach(var f in a.GetFailureMessages()) {if(f.GetSeverity()==FailureSeverity.Warning) a.DeleteWarning(f);else if(f.GetSeverity()==FailureSeverity.Error){System.IO.File.WriteAllText(@"C:\Users\admin\.codex\.chatgpt-projects\g-p-6acadee8e81c8191910d9dd6cf16b614\Capilla\scripts\refine\failure.txt",f.GetDescriptionText());return FailureProcessingResult.ProceedWithRollBack;}} return FailureProcessingResult.Continue; }
}
public static class Model {
 static Document D; static UIApplication UI;
 static string Root=@"C:\Users\admin\.codex\.chatgpt-projects\g-p-6acadee8e81c8191910d9dd6cf16b614\Capilla";
 static Dictionary<string,ElementId> Mats=new Dictionary<string,ElementId>();
 static Random R=new Random(241);
 static List<string> Notes=new List<string>();
 static double U=1/0.3048;
 static XYZ P(double x,double y,double z=0) {return new XYZ(x*U,y*U,z*U);}
 static ElementId E(long n){return new ElementId(n);}
 static double[] C(double x,double y,double z){return new[]{x,y,z};}
 static double Rand(double a,double b){return a+(b-a)*R.NextDouble();}
 static string Tag="CAPILLA_HD";
 static Level L {get{return new FilteredElementCollector(D).OfClass(typeof(Level)).Cast<Level>().First(l=>Math.Abs(l.Elevation)<.001);}}
 static readonly double[][] Buildings={new[]{0d,0,18,9},new[]{48d,18,10,6},new[]{-30d,6,10,8},new[]{-50d,12,7,6},new[]{14d,46,7,4},new[]{55d,50,7,5},new[]{-38d,-31,4,3},new[]{-24d,-60,8,5},new[]{26d,30,2.5,2.5}};
 static double Base(double x,double y) {return .135*y+.32*Math.Sin(x/18)*Math.Cos(y/24);}
 static double Height(double x,double y) {
  double h=Base(x,y)+.24*Math.Sin(x/11)*Math.Sin(y/15);
  for(int i=0;i<Buildings.Length;i++) {var b=Buildings[i];double cx=b[0],cy=b[1],wx=b[2]/2+.8,wy=b[3]/2+.8;
   if(i==0){cy=-1.3;wx=10;wy=6.8;} double dx=Math.Max(Math.Abs(x-cx)-wx,0),dy=Math.Max(Math.Abs(y-cy)-wy,0),dist=Math.Sqrt(dx*dx+dy*dy); if(dist<6){double t=dist/6; t=t*t*(3-2*t);h=(i==0?-.17:Base(b[0],b[1])-.17)*(1-t)+h*t;}
  }
  return h;
 }
 static T Get<T>(long id) where T:Element{return D.GetElement(E(id)) as T;}
 static void Tx(string title,Action action) {using(var t=new Transaction(D,"Capilla | "+title)){t.Start();var op=t.GetFailureHandlingOptions();op.SetFailuresPreprocessor(new Warnings());op.SetClearAfterRollback(true);t.SetFailureHandlingOptions(op); action();if(t.Commit()!=TransactionStatus.Committed)throw new Exception("Transaction failed: "+title);}}
 static void Del(long id){var e=D.GetElement(E(id));if(e!=null)D.Delete(e.Id);}
 static void Mark(Element e,string n){var p=e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);if(p!=null&&!p.IsReadOnly)p.Set(Tag+" | "+n+" | Proporciones estimadas del video; sin levantamiento");}
 static ElementId M(string name){return Mats[name];}
 static void LoadMats(){Mats=new FilteredElementCollector(D).OfClass(typeof(Material)).Cast<Material>().Where(x=>x.Name.StartsWith("Capilla HD - ")).ToDictionary(x=>x.Name.Substring(13),x=>x.Id);}
 static void Mat(string name,byte r,byte g,byte b,int transparency=0){
  string full="Capilla HD - "+name;var m=new FilteredElementCollector(D).OfClass(typeof(Material)).Cast<Material>().FirstOrDefault(x=>x.Name==full);if(m==null)m=Get<Material>(Material.Create(D,full).Value);
  m.Color=new Color(r,g,b);m.Transparency=transparency;m.Shininess=8;m.Smoothness=20;m.UseRenderAppearanceForShading=false;
  try{var lib=UI.Application.GetAssets(AssetType.Appearance);var generic=lib.FirstOrDefault(a=>a.FindByName("generic_diffuse")!=null);if(generic!=null){var a=AppearanceAssetElement.Create(D,full+" aspecto",generic);m.AppearanceAssetId=a.Id;using(var scope=new AppearanceAssetEditScope(D)){var asset=scope.Start(a.Id);var diffuse=asset.FindByName("generic_diffuse") as AssetPropertyDoubleArray4d;if(diffuse!=null){if(diffuse.NumberOfConnectedProperties>0)diffuse.RemoveConnectedAsset();diffuse.SetValueAsColor(new Color(r,g,b));}var gloss=asset.FindByName("generic_glossiness") as AssetPropertyDouble;if(gloss!=null)gloss.Value=name=="Agua"?.72:.14;var trans=asset.FindByName("generic_transparency") as AssetPropertyDouble;if(trans!=null)trans.Value=transparency/100.0;scope.Commit(true);}}}catch(Exception ex){Notes.Add("Apariencia "+name+": "+ex.Message);}
  Mats[name]=m.Id;
 }
 static void Materials(){Tx("materiales",()=>{
  Mat("Cesped",99,123,65);Mat("Tierra",119,105,79);Mat("Grava",138,132,113);Mat("Concreto",175,171,156);Mat("Concreto claro",207,205,187);Mat("Juntas",106,104,92);
  Mat("Estuco",213,202,173);Mat("Blanco",225,226,210);Mat("Zocalo",104,99,84);Mat("Piedra clara",146,142,121);Mat("Piedra oscura",99,104,92);
  Mat("Techo verde",112,145,132);Mat("Techo verde claro",128,155,140);Mat("Juntas cubierta",83,113,102);Mat("Metal gris",147,151,144);Mat("Teja",130,65,47);
  Mat("Madera",102,79,58);Mat("Baranda",172,155,126);Mat("Carpinteria",70,49,39);Mat("Vidrio",45,55,65,18);Mat("Cortina",87,82,123);
  Mat("Agua",54,74,45,10);Mat("Tronco",91,80,58);Mat("Tronco claro",167,167,132);Mat("Hoja oscura",39,65,32);Mat("Hoja media", sixty(),98,43);Mat("Hoja clara",88,119,53);Mat("Hoja dorada",139,157,83);Mat("Hoja crema",178,187,128);Mat("Flor violeta",146,36,124);Mat("Flor magenta",182,54,145);Mat("Borde vegetal",159,177,115);Mat("Hoja palma",84,112,50);
 });}
 static byte sixty(){return 60;}
 static void ApplyType<T>(long id,string name,string mat) where T:HostObjAttributes {var e=D.GetElement(E(id));if(e==null)return;var old=D.GetElement(e.GetTypeId()) as T;var type=new FilteredElementCollector(D).OfClass(typeof(T)).Cast<T>().FirstOrDefault(t=>t.Name==name)??(T)old.Duplicate(name);var cs=type.GetCompoundStructure();if(cs!=null){for(int j=0;j<cs.LayerCount;j++)cs.SetMaterialId(j,M(mat));type.SetCompoundStructure(cs);}e.ChangeTypeId(type.Id);}
 static CurveLoop Loop(IEnumerable<XYZ> p){var a=p.ToList();var l=new CurveLoop();for(int i=0;i<a.Count;i++)l.Append(Line.CreateBound(a[i],a[(i+1)%a.Count]));return l;}
 static Solid Box(double x,double y,double z,double w,double d,double h,string mat){return GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>{Loop(new[]{P(x,y,z),P(x+w,y,z),P(x+w,y+d,z),P(x,y+d,z)})},XYZ.BasisZ,h*U,new SolidOptions(M(mat),ElementId.InvalidElementId));}
 static Solid Rod(XYZ a,XYZ b,double radius,string mat,int sides=8){var dir=(b-a).Normalize();var v=dir.CrossProduct(Math.Abs(dir.Z)<.95?XYZ.BasisZ:XYZ.BasisX).Normalize();var w=dir.CrossProduct(v).Normalize();var points=Enumerable.Range(0,sides).Select(i=>a+radius*U*(Math.Cos(i*Math.PI*2/sides)*v+Math.Sin(i*Math.PI*2/sides)*w));return GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>{Loop(points)},dir,a.DistanceTo(b),new SolidOptions(M(mat),ElementId.InvalidElementId));}
 static DirectShape Shape(string name,IList<GeometryObject> geometry,BuiltInCategory cat=BuiltInCategory.OST_GenericModel){var cid=new ElementId(cat);if(!DirectShape.IsValidCategoryId(cid,D))cid=new ElementId(BuiltInCategory.OST_GenericModel);var ds=DirectShape.CreateElement(D,cid);ds.ApplicationId=Tag;ds.ApplicationDataId=name;ds.Name="Capilla - "+name;ds.SetShape(geometry);Mark(ds,name);return ds;}
 static void Move(long id,double z){if(Math.Abs(z)<.00001)return;var e=D.GetElement(E(id));if(e!=null)ElementTransformUtils.MoveElement(D,e.Id,P(0,0,z));}
 static void Terrain(){
  var old=JObject.Parse(File.ReadAllText(Path.Combine(Root,"scripts/refine/before.json")));
  Tx("relieve y cotas del conjunto",()=>{
   foreach(var w in new FilteredElementCollector(D).OfClass(typeof(Wall)).Cast<Wall>())foreach(var a in w.GetAttachmentIds(AttachmentLocation.Top).ToList())w.RemoveAttachment(a);
   var floors=new long[]{154640,154738,154747,154755,154763,154771,154779,154788,154804};var roofs=new long[]{155320,155354,155386,155403,155420,155437,155456,155490,155522};
   for(int i=1;i<Buildings.Length;i++){double z=Base(Buildings[i][0],Buildings[i][1]);for(int k=0;k<4;k++)Move(154891+(i-1)*4+k,z);Move(floors[i],z);Move(roofs[i],z);}
   foreach(long id in new long[]{154632,154656,154667,154682,154699,154711,154721,154730,154813,154833,154853,154873,154796,155622})Del(id);
   foreach(var fi in new FilteredElementCollector(D).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().Where(f=>f.Symbol.FamilyName.StartsWith("Finca ")).ToList())D.Delete(fi.Id);
   var tt0=new FilteredElementCollector(D).OfClass(typeof(ToposolidType)).Cast<ToposolidType>().First();var tt=(ToposolidType)tt0.Duplicate("Capilla HD - relieve de finca estimado");var cs=tt.GetCompoundStructure();for(int i=0;i<cs.LayerCount;i++)cs.SetMaterialId(i,M("Cesped"));tt.SetCompoundStructure(cs);
   var pts=new Dictionary<string,XYZ>();Action<double,double> add=(x,y)=>pts[x.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+","+y.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)]=P(x,y,Height(x,y));
   for(double x=-110;x<=110;x+=5)for(double y=-95;y<=105;y+=5)add(x,y);
   for(double x=-16;x<=16;x+=2)for(double y=-12;y<=10;y+=2)add(x,y);
   var topo=Toposolid.Create(D,pts.Values.ToList(),tt.Id,L.Id);Mark(topo,"Topografia visual estimada del video");
   foreach(var fi in new FilteredElementCollector(D).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().Where(f=>f.Host is Wall)){
    var loc=fi.Location as LocationPoint;if(loc==null)continue;var p=loc.Point*.3048;var host=fi.Host as Wall;var b=host.get_BoundingBox(null);if(b!=null){double baseZ=b.Min.Z*.3048;double sill=fi.Category.Id.Value==(long)BuiltInCategory.OST_Windows?1.0:0;double delta=baseZ+sill-p.Z;if(Math.Abs(delta)>.05)ElementTransformUtils.MoveElement(D,fi.Id,P(0,0,delta));}
   }
  });
 }
 static void Architecture(){Tx("arquitectura y cubierta de la casa",()=>{
  foreach(long id in new long[]{154879,154882,154885,154888})ApplyType<WallType>(id,"Capilla HD - estuco crema","Estuco");
  foreach(var w in new FilteredElementCollector(D).OfClass(typeof(Wall)).Cast<Wall>().Where(w=>w.Id.Value>=154891&&w.Id.Value<=154922).ToList())ApplyType<WallType>(w.Id.Value,"Capilla HD - estuco blanco","Blanco");
  foreach(var f in new FilteredElementCollector(D).OfClass(typeof(Floor)).Cast<Floor>().ToList())ApplyType<FloorType>(f.Id.Value,"Capilla HD - pavimento concreto","Concreto");
  Del(155320);
  var rt0=Get<RoofType>(1400);var rt=(RoofType)rt0.Duplicate("Capilla HD - cubierta verde casa principal");var cs=rt.GetCompoundStructure();for(int i=0;i<cs.LayerCount;i++)cs.SetMaterialId(i,M("Techo verde"));rt.SetCompoundStructure(cs);
  var ca=new CurveArray();var pts=new[]{P(-10,-8.05),P(10,-8.05),P(10,5.5),P(-10,5.5)};for(int i=0;i<4;i++)ca.Append(Line.CreateBound(pts[i],pts[(i+1)%4]));ModelCurveArray curves=new ModelCurveArray();var roof=D.Create.NewFootPrintRoof(ca,L,rt,out curves);roof.get_Parameter(BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM).Set(3.08*U);foreach(ModelCurve mc in curves){roof.set_DefinesSlope(mc,true);roof.set_SlopeAngle(mc,.23);}Mark(roof,"Cubierta cuatro aguas con corredor cubierto");
  foreach(var r in new FilteredElementCollector(D).OfClass(typeof(Autodesk.Revit.DB.Architecture.Railing)).ToList())D.Delete(r.Id);
  var veranda=new List<GeometryObject>();
  for(double x=-9;x<=9.01;x+=3){veranda.Add(Box(x-.065,-7.45,.05,.13,.13,2.95,"Baranda"));veranda.Add(Box(x-.13,-7.51,.05,.26,.26,.20,"Zocalo"));}
  veranda.Add(Box(-9.1,-7.46,2.85,18.2,.20,.22,"Madera"));
  for(double y=-7.3;y<4.6;y+=3){veranda.Add(Box(9.0,y,0,.13,.13,3.0,"Baranda"));}
  for(int side=-1;side<=1;side+=2){double xa=side<0?-9:1,xb=side<0?-1:9;
   veranda.Add(Box(xa,-7.40,.93,xb-xa,.09,.10,"Baranda"));veranda.Add(Box(xa,-7.40,.21,xb-xa,.075,.075,"Baranda"));
   for(double x=xa+.12;x<xb;x+=.17)veranda.Add(Box(x,-7.38,.28,.04,.045,.66,"Baranda"));
   veranda.Add(Box(side*9-.035,-7.35,.93,.075,2.85,.10,"Baranda"));for(double y=-7.3;y<-4.5;y+=.18)veranda.Add(Box(side*9-.02,y,.28,.04,.04,.66,"Baranda"));
  }
  for(double x=-8.9;x<=9;x+=1.5)veranda.Add(Box(x,-7.35,2.89,.075,2.9,.11,"Madera"));
  Shape("Corredor - postes vigas y baranda de madera",veranda);
  var basegeo=new List<GeometryObject>();basegeo.Add(Box(-9.05,-7.55,-.6,18.1,3.12,.62,"Zocalo"));
  for(double x=-9;x<9;x+=.48){double w=Rand(.28,.48);for(int row=0;row<2;row++)basegeo.Add(Box(x,-7.565,-.52+row*.26,w,.055,.20,((int)(x*10)+row)%2==0?"Piedra clara":"Piedra oscura"));}
  Shape("Zocalo de piedra del corredor",basegeo);
  var edges=new List<GeometryObject>();edges.Add(Box(-10,-8.07,3.05,20,.12,.16,"Baranda"));edges.Add(Box(-10,5.45,3.05,20,.12,.16,"Baranda"));edges.Add(Box(-10.05,-8.05,3.05,.12,13.55,.16,"Baranda"));edges.Add(Box(9.94,-8.05,3.05,.12,13.55,.16,"Baranda"));
  foreach(double x in new[]{-9.2,9.2})edges.Add(Rod(P(x,-7.75,3.02),P(x,-7.75,-.05),.045,"Metal gris",10));
  Shape("Aleros canalones y bajantes",edges);
  // Seams follow the four native roof slopes; all measurements here are metres.
  Func<double,double,double> rz=(x,y)=>3.08+.23*Math.Min(Math.Min(x+10,10-x),Math.Min(y+8.05,5.5-y))+.145;
  var seams=new List<GeometryObject>();for(double x=-9.75;x<10;x+=.76){for(double y=-7.96;y<5.48;y+=.45){double yn=Math.Min(y+.45,5.48);seams.Add(Rod(P(x,y,rz(x,y)),P(x,yn,rz(x,yn)),.012,"Juntas cubierta",4));}}
  double ridgeY=(-8.05+5.5)/2,half=(5.5+8.05)/2;seams.Add(Rod(P(-10+half,ridgeY,rz(-10+half,ridgeY)+.035),P(10-half,ridgeY,rz(10-half,ridgeY)+.035),.075,"Techo verde claro",8));
  foreach(var xy in new[]{new[]{-10d,-8.05},new[]{10d,-8.05},new[]{10d,5.5},new[]{-10d,5.5}}){double xr=xy[0]<0?-10+half:10-half;seams.Add(Rod(P(xy[0],xy[1],rz(xy[0],xy[1])+.03),P(xr,ridgeY,rz(xr,ridgeY)+.03),.035,"Techo verde claro",6));}
  Shape("Juntas de laminas limatesas y cumbrera",seams);
  var frames=new List<GeometryObject>();foreach(double x in new[]{-5.4,-2.7,3.24,5.76}){frames.Add(Box(x-.53,-4.64,.94,1.06,.12,.07,"Carpinteria"));frames.Add(Box(x-.53,-4.64,2.22,1.06,.12,.07,"Carpinteria"));foreach(double xx in new[]{x-.53,x+.48,x-.025})frames.Add(Box(xx,-4.65,1.01,.055,.12,1.21,"Carpinteria"));frames.Add(Box(x-.43,-4.54,1.1,.32,.015,1.07,"Cortina"));}
  foreach(double x in new[]{-1.1,1.1})frames.Add(Box(x-.05,-4.65,0,.10,.13,2.3,"Carpinteria"));frames.Add(Box(-1.15,-4.65,2.27,2.30,.13,.10,"Carpinteria"));
  Shape("Carpinterias y cortinas de fachada",frames);
  foreach(var r in new FilteredElementCollector(D).OfClass(typeof(RoofBase)).Cast<RoofBase>().Where(r=>r.Id!=roof.Id).ToList())ApplyType<RoofType>(r.Id.Value,"Capilla HD - "+(r.Name.Contains("roja")?"teja roja":"lamina gris"),r.Name.Contains("roja")?"Teja":"Metal gris");
 });}
 class Mesh {
  public List<Tuple<XYZ[],string>> Faces=new List<Tuple<XYZ[],string>>();
  public void Tri(XYZ a,XYZ b,XYZ c,string mat){if((b-a).CrossProduct(c-a).GetLength()>1e-7)Faces.Add(Tuple.Create(new[]{a,b,c},mat));}
  public void Quad(XYZ a,XYZ b,XYZ c,XYZ d,string mat){Tri(a,b,c,mat);Tri(a,c,d,mat);}
  public void Tube(XYZ a,XYZ b,double r0,double r1,string mat,int n=7){var dir=(b-a).Normalize();var v=dir.CrossProduct(Math.Abs(dir.Z)<.95?XYZ.BasisZ:XYZ.BasisX).Normalize();var w=dir.CrossProduct(v).Normalize();for(int j=0;j<n;j++){double t=2*Math.PI*j/n,q=2*Math.PI*(j+1)/n;var u0=Math.Cos(t)*v+Math.Sin(t)*w;var u1=Math.Cos(q)*v+Math.Sin(q)*w;Quad(a+r0*U*u0,a+r0*U*u1,b+r1*U*u1,b+r1*U*u0,mat);}}
  public void Ell(double x,double y,double z,double rx,double ry,double rz,string mat,int seed,int n=12,int rows=7){
   var grid=new XYZ[rows+1,n];double phase=seed*.713;
   for(int i=0;i<=rows;i++)for(int j=0;j<n;j++){double th=Math.PI*i/rows,ph=2*Math.PI*j/n;double rough=1+.10*Math.Sin(ph*5+phase)*Math.Sin(th*3)+.06*Math.Cos(ph*9-th*4+phase);grid[i,j]=P(x+rx*Math.Sin(th)*Math.Cos(ph)*rough,y+ry*Math.Sin(th)*Math.Sin(ph)*rough,z+rz*Math.Cos(th)*rough);}
   for(int i=0;i<rows;i++)for(int j=0;j<n;j++){int k=(j+1)%n;Quad(grid[i,j],grid[i,k],grid[i+1,k],grid[i+1,j],mat);}
  }
  public IList<GeometryObject> Build(){var geo=new List<GeometryObject>();foreach(var group in Faces.GroupBy(f=>f.Item2)){using(var b=new TessellatedShapeBuilder()){b.OpenConnectedFaceSet(false);foreach(var f in group)b.AddFace(new TessellatedFace(f.Item1,M(f.Item2)));b.CloseConnectedFaceSet();b.Target=TessellatedShapeBuilderTarget.Mesh;b.Fallback=TessellatedShapeBuilderFallback.Salvage;b.Build();geo.AddRange(b.GetBuildResult().GetGeometricalObjects());}}return geo;}
 }
 static List<double[]> Sample(double[][] path,double spacing){var list=new List<double[]>();for(int i=0;i<path.Length-1;i++){var a=path[i];var b=path[i+1];double d=Math.Sqrt(Math.Pow(b[0]-a[0],2)+Math.Pow(b[1]-a[1],2));int n=(int)Math.Ceiling(d/spacing);for(int j=0;j<n;j++){double t=(double)j/n;list.Add(new[]{a[0]+(b[0]-a[0])*t,a[1]+(b[1]-a[1])*t});}}list.Add(path.Last());return list;}
 static List<double[]> Offset(List<double[]> line,double offset){var p=new List<double[]>();for(int i=0;i<line.Count;i++){var a=line[Math.Max(0,i-1)];var b=line[Math.Min(line.Count-1,i+1)];double dx=b[0]-a[0],dy=b[1]-a[1],len=Math.Sqrt(dx*dx+dy*dy);p.Add(new[]{line[i][0]-dy/len*offset,line[i][1]+dx/len*offset});}return p;}
 static void Pavement(string name,List<double[]> line,double width,string mat,double raised=.14){var a=Offset(line,width/2);var b=Offset(line,-width/2);b.Reverse();a.AddRange(b);var type=new FilteredElementCollector(D).OfClass(typeof(FloorType)).Cast<FloorType>().FirstOrDefault(f=>f.Name=="Capilla HD - camino "+mat);if(type==null){type=(FloorType)Get<FloorType>(1238).Duplicate("Capilla HD - camino "+mat);var cs=type.GetCompoundStructure();for(int i=0;i<cs.LayerCount;i++){cs.SetMaterialId(i,M(mat));if(i==0)cs.SetLayerWidth(i,.10*U);}type.SetCompoundStructure(cs);}var f=Floor.Create(D,new List<CurveLoop>{Loop(a.Select(p=>P(p[0],p[1])))},type.Id,L.Id);D.Regenerate();var ed=f.GetSlabShapeEditor();ed.Enable();D.Regenerate();foreach(SlabShapeVertex v in ed.SlabShapeVertices){var p=v.Position*.3048;ed.ModifySubElement(v,(Height(p.X,p.Y)+raised)*U);}Mark(f,name);}
 static void Ribbon(Mesh mesh,List<double[]> line,double width,string mat,double off=.16){var left=Offset(line,width/2);var right=Offset(line,-width/2);for(int i=0;i<line.Count-1;i++){var a=left[i];var b=right[i];var c=right[i+1];var d=left[i+1];mesh.Quad(P(a[0],a[1],Height(a[0],a[1])+off),P(b[0],b[1],Height(b[0],b[1])+off),P(c[0],c[1],Height(c[0],c[1])+off),P(d[0],d[1],Height(d[0],d[1])+off),mat);}}
 static void Tuft(Mesh mesh,double x,double y,double size,string mat){double z=Height(x,y)+.04;for(int j=0;j<11;j++){double angle=Rand(0,Math.PI*2),len=Rand(.18,.42)*size,spread=Rand(.12,.3)*size;var a=P(x,y,z);var b=P(x+Math.Cos(angle)*spread*.35,y+Math.Sin(angle)*spread*.35,z+len);var tip=P(x+Math.Cos(angle)*spread,y+Math.Sin(angle)*spread,z+len*.6);var cross=P(-Math.Sin(angle)*.025*size,Math.Cos(angle)*.025*size,0);mesh.Quad(a,b-cross,tip,b+cross,mat);}}
 static void Landscape(){Tx("senderos estanque y jardines",()=>{
  var central=Sample(new[]{new[]{0d,-7.55},new[]{-5d,-16},new[]{-12d,-30}},1.2);Pavement("Acceso central",central,1.5,"Concreto claro",.19);
  var road=Sample(new[]{new[]{-47d,-36.4},new[]{-25d,-32},new[]{-12d,-30},new[]{5d,-27},new[]{15d,-24}},1.8);Pavement("Via - franja drenante",road,3.55,"Grava",.13);Pavement("Via - huella izquierda",Offset(road,-.98),.92,"Concreto",.16);Pavement("Via - huella derecha",Offset(road,.98),.92,"Concreto",.16);
  var drive=Sample(new[]{new[]{15d,-24},new[]{21d,-15},new[]{26d,-3},new[]{32d,11},new[]{41d,21},new[]{53d,27}},1.5);Pavement("Acceso vehicular curvado",drive,3.8,"Tierra",.16);
  var upper=Sample(new[]{new[]{8d,6},new[]{9d,14},new[]{7.8,21},new[]{10d,29}},1.2);Pavement("Sendero posterior",upper,1.4,"Concreto claro",.16);
  var lower=Sample(new[]{new[]{-12d,-30},new[]{-13.5,-34},new[]{-17d,-38},new[]{-18d,-42},new[]{-15d,-45}},1.2);Pavement("Sendero a deposito circular",lower,1.1,"Concreto claro",.16);
  var border=new Mesh();foreach(var l in new[]{central,road,upper,lower}){double width=l==road?3.55:l==central?1.5:l==upper?1.4:1.1;Ribbon(border,Offset(l,width/2+.035),.085,"Blanco",.22);Ribbon(border,Offset(l,-width/2-.035),.085,"Blanco",.22);}
  foreach(var l in new[]{road,central})for(int i=1;i<l.Count-1;i++){var cross=Offset(l,.1);var a=l[i-1];var b=l[i+1];double dx=b[0]-a[0],dy=b[1]-a[1],len=Math.Sqrt(dx*dx+dy*dy),nx=-dy/len,ny=dx/len;double half=l==road?1.72:.72;double x=l[i][0],y=l[i][1];border.Quad(P(x+nx*half,y+ny*half,Height(x+nx*half,y+ny*half)+.195),P(x-nx*half,y-ny*half,Height(x-nx*half,y-ny*half)+.195),P(x-nx*half+dx/len*.018,y-ny*half+dy/len*.018,Height(x-nx*half,y-ny*half)+.195),P(x+nx*half+dx/len*.018,y+ny*half+dy/len*.018,Height(x+nx*half,y+ny*half)+.195),"Juntas");}
  Shape("Bordillos y juntas de pavimentos",border.Build());
  var hedges=new Mesh();foreach(var side in new[]{-1d,1d}){foreach(var q in Offset(Sample(new[]{new[]{0d,-7.8},new[]{-5d,-16},new[]{-12d,-29.2}},.38),side*1.03))Tuft(hedges,q[0],q[1],1,"Borde vegetal");foreach(var q in Offset(Sample(new[]{new[]{8d,6},new[]{9d,14},new[]{7.8,21},new[]{10d,29}},.42),side*.95))Tuft(hedges,q[0],q[1],1,"Borde vegetal");}Shape("Borduras vegetales del recorrido principal",hedges.Build(),BuiltInCategory.OST_Planting);
  var steps=new List<GeometryObject>();for(int i=0;i<4;i++){double x=-12+.19*i,y=-30+.33*i;double z=Height(x,y)+.23;steps.Add(Box(x-.7,y-.16,z-.22,1.4,.40,.22,"Concreto claro"));}Shape("Peldanos de acceso desde la via",steps);
  var garden=new Mesh();foreach(var c in new[]{new[]{10d,30,3.5},new[]{6d,12,2d},new[]{-15d,-48,2.8},new[]{-28d,-17,1.0}}){double z=Height(c[0],c[1])+.12;for(int i=0;i<64;i++){double a=i*Math.PI/32,b=(i+1)*Math.PI/32;double r=c[2];garden.Quad(P(c[0]+r*Math.Cos(a),c[1]+r*Math.Sin(a),z),P(c[0]+r*Math.Cos(b),c[1]+r*Math.Sin(b),z),P(c[0]+r*Math.Cos(b),c[1]+r*Math.Sin(b),z+.38),P(c[0]+r*Math.Cos(a),c[1]+r*Math.Sin(a),z+.38),"Blanco");garden.Quad(P(c[0]+(r-.13)*Math.Cos(a),c[1]+(r-.13)*Math.Sin(a),z+.38),P(c[0]+(r-.13)*Math.Cos(b),c[1]+(r-.13)*Math.Sin(b),z+.38),P(c[0]+r*Math.Cos(b),c[1]+r*Math.Sin(b),z+.38),P(c[0]+r*Math.Cos(a),c[1]+r*Math.Sin(a),z+.38),"Concreto claro");if(c[0]==-15)garden.Tri(P(c[0],c[1],z+.12),P(c[0]+r*Math.Cos(a),c[1]+r*Math.Sin(a),z+.12),P(c[0]+r*Math.Cos(b),c[1]+r*Math.Sin(b),z+.12),"Concreto claro");}}
  Shape("Jardineras y deposito circular",garden.Build());
  var topo=new FilteredElementCollector(D).OfClass(typeof(Toposolid)).Cast<Toposolid>().First();var topoType=topo.GetTypeId();D.Delete(topo.Id);var terrainPts=new Dictionary<string,XYZ>();Action<double,double> add=(x,y)=>{double r=Math.Sqrt(Math.Pow((x-2)/5.5,2)+Math.Pow((y+50)/3.8,2));double h=Height(x,y);if(r<1.4){double t=Math.Max(0,Math.Min(1,(r-.82)/.58));t=t*t*(3-2*t);h=-8.1*(1-t)+h*t;}terrainPts[x.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+","+y.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)]=P(x,y,h);};for(double x=-110;x<=110;x+=5)for(double y=-95;y<=105;y+=5)add(x,y);for(double x=-16;x<=16;x+=2)for(double y=-12;y<=10;y+=2)add(x,y);for(double x=-6;x<=10;x+=1)for(double y=-57;y<=-43;y+=1)add(x,y);var newTopo=Toposolid.Create(D,terrainPts.Values.ToList(),topoType,L.Id);Mark(newTopo,"Relieve y vaso de estanque aproximados");
  var pond=new Mesh();for(int i=0;i<64;i++){double a=2*Math.PI*i/64,b=2*Math.PI*(i+1)/64;pond.Tri(P(2,-50,-7.22),P(2+5*Math.Cos(a),-50+3.2*Math.Sin(a),-7.22),P(2+5*Math.Cos(b),-50+3.2*Math.Sin(b),-7.22),"Agua");}Shape("Estanque - lamina de agua",pond.Build());
  var fence=new List<GeometryObject>();var fenceLine=Sample(new[]{new[]{-49d,-44},new[]{-46d,-25},new[]{-42d,-10},new[]{-39d,12}},3);for(int i=0;i<fenceLine.Count;i++){var q=fenceLine[i];double z=Height(q[0],q[1]);fence.Add(Rod(P(q[0],q[1],z-.1),P(q[0],q[1],z+1.1),.05,"Blanco",6));if(i>0){var a=fenceLine[i-1];for(double zz=.4;zz<=.95;zz+=.25)fence.Add(Rod(P(a[0],a[1],Height(a[0],a[1])+zz),P(q[0],q[1],z+zz),.007,"Metal gris",4));}}Shape("Cerramiento rural de postes blancos",fence);
  for(long i=154923;i<=154926;i++)Del(i);var shrine=new List<GeometryObject>();double sz=Base(26,30);shrine.Add(Box(21.5,26.5,sz-.12,9,7,.23,"Concreto claro"));foreach(var seg in new[]{new[]{21.5,26.5,24.6,26.5},new[]{27.4,26.5,30.5,26.5},new[]{30.5,26.5,30.5,33.5},new[]{30.5,33.5,21.5,33.5},new[]{21.5,33.5,21.5,26.5}}){var a=P(seg[0],seg[1],sz);var b=P(seg[2],seg[3],sz);int n=(int)(a.DistanceTo(b)/U/1.6)+1;for(int j=0;j<=n;j++){var p=a+(b-a)*(double)j/n;shrine.Add(Rod(p,p+P(0,0,1),.045,"Blanco",6));}foreach(double zz in new[]{.45,.92})shrine.Add(Rod(a+P(0,0,zz),b+P(0,0,zz),.035,"Blanco",6));}Shape("Plataforma y cerca del santuario",shrine);
 });}
 static void Tree(string kind,double x,double y,double h,double rad,int seed){
  R=new Random(seed);var mesh=new Mesh();double ground=Height(x,y)-.07;
  if(kind=="Palma"||kind=="Platano"){
   bool banana=kind=="Platano";double trunkH=h*.78;double bend=Rand(-.25,.25);for(int k=0;k<5;k++){double a=k/5d,b=(k+1)/5d;mesh.Tube(P(x+bend*a*a,y,ground+trunkH*a),P(x+bend*b*b,y,ground+trunkH*b),banana?.11:.10,banana?.1:.08,banana?"Hoja palma":"Tronco claro",9);}
   var crown=P(x+bend,y,ground+trunkH);int count=banana?9:13;
   for(int k=0;k<count;k++){double angle=k*Math.PI*2/count+Rand(-.14,.14),len=rad*Rand(.82,1.2);var dir=new XYZ(Math.Cos(angle),Math.Sin(angle),0);var side=new XYZ(-Math.Sin(angle),Math.Cos(angle),0);var line=new List<XYZ>();for(int j=0;j<=12;j++){double t=j/12d;line.Add(crown+dir*len*t*U+XYZ.BasisZ*(Math.Sin(t*Math.PI)*len*.34-t*t*len*.31+.12)*U);}for(int j=0;j<12;j++){mesh.Tube(line[j],line[j+1],.013,.009,"Hoja dorada",5);if(banana){double w=.45*Math.Sin(Math.PI*(j+.5)/12);mesh.Quad(line[j],line[j]+side*w*U-XYZ.BasisZ*.08*U,line[j+1]+side*w*U-XYZ.BasisZ*.12*U,line[j+1],"Hoja palma");mesh.Quad(line[j],line[j+1],line[j+1]-side*w*U-XYZ.BasisZ*.12*U,line[j]-side*w*U-XYZ.BasisZ*.08*U,"Hoja media");}else {for(int q=0;q<3;q++){double t=(j+q/3d)/12,wide=.6*Math.Sin(Math.PI*t)*Rand(.8,1.05);var root=line[j]+(line[j+1]-line[j])*(q/3d);foreach(int sign in new[]{-1,1}){var tip=root+side*sign*wide*U+dir*.24*U-XYZ.BasisZ*.12*U;var mid=(root+tip)/2+XYZ.BasisZ*.035*U;mesh.Quad(root,mid-dir*.034*U,tip,mid+dir*.034*U,((j+k)%3==0)?"Hoja clara":"Hoja palma");}}}}
   }
  }else if(kind=="Pino"){
   mesh.Tube(P(x,y,ground),P(x,y,ground+h),.23,.045,"Tronco",11);
   for(int row=0;row<22;row++){double t=row/22d,z=ground+h*(.12+.87*t),r=rad*Math.Pow(1-t,.8);int count=8+(row%3);for(int k=0;k<count;k++){double a=k*Math.PI*2/count+row*.61;var start=P(x,y,z);var dir=new XYZ(Math.Cos(a),Math.Sin(a),0);var side=new XYZ(-Math.Sin(a),Math.Cos(a),0);var end=start+dir*r*U-XYZ.BasisZ*.22*U;mesh.Tube(start,end,.035*(1-t)+.009,.005,"Tronco",5);for(int s=1;s<=7;s++){double q=s/7d;var node=start+(end-start)*q;foreach(int sign in new[]{-1,1}){double reach=r*.36*(1-q*.7)+.12;var branch=node+side*sign*reach*U+dir*.18*U-XYZ.BasisZ*.11*U;mesh.Tube(node,branch,.009,.003,"Hoja oscura",4);for(int j=0;j<9;j++){double v=j/9d;var n=node+(branch-node)*v;double width=.12*(1-v*.55);var tip=n+dir*.13*U+XYZ.BasisZ*.04*U;mesh.Quad(n,tip+side*width*U,n+dir*.22*U,tip-side*width*U,(k+row+s)%4==0?"Hoja media":"Hoja oscura");}}}}
   }
  }else if(kind=="Cipres"){
   mesh.Tube(P(x,y,ground),P(x,y,ground+h*.9),.07,.02,"Tronco",7);for(int i=0;i<15;i++){double t=i/15d,r=rad*(1-.8*t);mesh.Ell(x+Rand(-.08,.08),y+Rand(-.08,.08),ground+h*(.12+.85*t),r,r,h*.11,i%3==0?"Hoja media":"Hoja oscura",seed+i,10,6);}
  }else if(kind=="Arbusto"||kind=="Buganvilla"){
   mesh.Tube(P(x,y,ground),P(x,y,ground+h*.7),.08,.025,"Tronco",6);for(int i=0;i<18;i++){double a=Rand(0,Math.PI*2),rr=rad*Math.Sqrt(R.NextDouble())*.65;string mat=kind=="Buganvilla"?(i%3==0?"Hoja media":"Hoja oscura"):(i%3==0?"Hoja dorada":"Hoja media");mesh.Ell(x+Math.Cos(a)*rr,y+Math.Sin(a)*rr,ground+h*(.40+Rand(0,.4)),rad*.39,rad*.39,h*.28,mat,seed+i,22,12);}
   if(kind=="Buganvilla")for(int i=0;i<950;i++){double a=Rand(0,Math.PI*2),t=Rand(-.7,.85),radius=rad*Math.Sqrt(1-t*t)*Rand(.72,1.04);var c=P(x+Math.Cos(a)*radius,y+Math.Sin(a)*radius,ground+h*(.58+.38*t));var normal=new XYZ(Math.Cos(a),Math.Sin(a),t).Normalize();var u=normal.CrossProduct(XYZ.BasisZ).Normalize();var v=normal.CrossProduct(u).Normalize();double s=Rand(.03,.065)*U;for(int j=0;j<3;j++){double ph=j*Math.PI*2/3;var tip=c+(u*Math.Cos(ph)+v*Math.Sin(ph))*s;mesh.Tri(c,tip+v*s*.4,tip-v*s*.4,i%3==0?"Flor magenta":"Flor violeta");}}
  }else{
   bool pale=kind=="Arbol crema",forest=kind=="Bosque",euc=kind=="Eucalipto";double crown=euc?.78:.59;double trunk=euc?.12:rad*.085;mesh.Tube(P(x,y,ground),P(x+rad*.07,y,ground+h*crown),trunk,trunk*.38,euc?"Tronco claro":"Tronco",10);
   int n=forest?14:euc?9:24;for(int i=0;i<n;i++){double a=Rand(0,Math.PI*2),r=rad*Math.Sqrt(R.NextDouble())*.78,z=ground+h*(crown+Rand(-.08,.18));double xx=x+Math.Cos(a)*r,yy=y+Math.Sin(a)*r;mesh.Tube(P(x+rad*.04,y,ground+h*(crown-.15)),P(xx,yy,z),trunk*.32,.025,euc?"Tronco claro":"Tronco",6);string mat=pale?(i%3==0?"Hoja dorada":"Hoja crema"):i%4==0?"Hoja clara":i%3==0?"Hoja oscura":"Hoja media";mesh.Ell(xx,yy,z,rad*.48,rad*.43,h*(euc?.08:.16),mat,seed+i,forest?16:28,forest?10:16);}
   if(pale)mesh.Tube(P(x,y,ground),P(x,y,ground+1.0),trunk*1.02,trunk*.9,"Blanco",10);
  }
  Shape(kind+" "+seed,mesh.Build(),BuiltInCategory.OST_Planting);
 }
 static List<Tuple<string,double,double,double,double,int>> Plants(){
   var p=new List<Tuple<string,double,double,double,double,int>>();int seed=1400;
  Action<string,double,double,double,double> add=(k,x,y,h,r)=>p.Add(Tuple.Create(k,x,y,h,r,seed++));
  add("Pino",7,-8,13.8,2.8);add("Pino",-9,16,12.8,2.6);add("Pino",18,40,15,2.8);
  add("Arbol crema",-13,-10,6.8,3.0);add("Arbol crema",-23,-21,5.2,2.5);add("Arbol crema",-28,-17,3.6,1.65);
  foreach(double x in new[]{-35d,-27,-19,-7,2,10})add("Cipres",x,-30+(x+35)*.17,6.5+(x%3)*.16,.66);
  foreach(var q in new[]{new[]{-8d,-36},new[]{-1d,-34},new[]{7d,-33},new[]{12d,-31},new[]{-3d,-41},new[]{-18d,-38}})add("Palma",q[0],q[1],6.0,2.5);
  foreach(var q in new[]{new[]{10.8,-6.0},new[]{12d,-2.5},new[]{11.8,1.5},new[]{10.5,5.8},new[]{-10.6,-7.8}})add("Buganvilla",q[0],q[1],q[0]<0?1.4:2.7,q[0]<0?.75:1.45);
  foreach(var q in new[]{new[]{-44d,-46},new[]{29d,-29},new[]{43d,-16},new[]{58d,-32},new[]{-51d,-20},new[]{63d,5}})add("Arbol",q[0],q[1],9,3.9);
  add("Arbol",10,30,6.4,2.8);add("Arbusto",6,12,1.6,1.0);
  var rnd=new Random(987);for(int i=0;i<38;i++){double x=-35+rnd.NextDouble()*95,y=-55+rnd.NextDouble()*103;
   if(Buildings.Any(b=>Math.Abs(x-b[0])<b[2]/2+3&&Math.Abs(y-b[1])<b[3]/2+3))continue;if(x>-15&&x<3&&y>-32&&y<-8)continue;if(x>12&&x<45&&y>-30&&y<25)continue;add("Arbusto",x,y,1.0+rnd.NextDouble()*.7,.65+rnd.NextDouble()*.45);}
  for(int i=0;i<18;i++)add("Platano",30+rnd.NextDouble()*32,-70+rnd.NextDouble()*26,4.5+rnd.NextDouble()*1.8,1.8);
  for(int i=0;i<10;i++)add("Eucalipto",-3+i*3.1,57+rnd.NextDouble()*8,19+rnd.NextDouble()*6,2.0);
  for(double x=-85;x<=85;x+=9)for(double y=-75;y<=86;y+=10){if((x>-53&&x<70&&y<59)||rnd.NextDouble()<.20)continue;double px=x+rnd.NextDouble()*4,py=y+rnd.NextDouble()*4;add("Bosque",px,py,9+rnd.NextDouble()*6,4+rnd.NextDouble()*1.6);}
  return p;
 }
 static void Planting(int part){var plants=Plants();int start=part*20;Tx("vegetacion detallada lote "+part,()=>{for(int i=start;i<Math.Min(plants.Count,start+20);i++){var p=plants[i];var existing=new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(x=>x.ApplicationId==Tag&&x.ApplicationDataId==p.Item1+" "+p.Item6).ToList();foreach(var e in existing)D.Delete(e.Id);Tree(p.Item1,p.Item2,p.Item3,p.Item4,p.Item5,p.Item6);}});Notes.Add("Plantas "+start+".."+Math.Min(plants.Count,start+20)+" de "+plants.Count);}
 static View3D Camera(string name,double[] eye,double[] target,double[] min,double[] max){var v=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().FirstOrDefault(x=>x.Name==name);if(v==null){var vt=new FilteredElementCollector(D).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().First(x=>x.ViewFamily==ViewFamily.ThreeDimensional);v=View3D.CreateIsometric(D,vt.Id);v.Name=name;}var e=P(eye[0],eye[1],eye[2]);var f=(P(target[0],target[1],target[2])-e).Normalize();var r=f.CrossProduct(XYZ.BasisZ).Normalize();v.SetOrientation(new ViewOrientation3D(e,r.CrossProduct(f),f));v.SetSectionBox(new BoundingBoxXYZ{Min=P(min[0],min[1],min[2]),Max=P(max[0],max[1],max[2])});v.IsSectionBoxActive=true;v.DisplayStyle=DisplayStyle.Realistic;v.DetailLevel=ViewDetailLevel.Fine;v.SunlightIntensity=75;v.ShadowIntensity=35;
  foreach(var c in new[]{BuiltInCategory.OST_Levels,BuiltInCategory.OST_Grids,BuiltInCategory.OST_Sections,BuiltInCategory.OST_Elev,BuiltInCategory.OST_SectionBox})try{v.SetCategoryHidden(new ElementId(c),true);}catch{}
  var model=v.GetViewDisplayModel();model.SmoothEdges=true;model.EnableSilhouettes=false;v.SetViewDisplayModel(model);
  try{v.SetBackground(ViewDisplayBackground.CreateGradient(new Color(184,205,215),new Color(235,236,219),new Color(153,166,119)));}catch{}
  return v;
 }
 static void Views(){Tx("vistas de presentacion",()=>{
  Camera("CAPILLA - 01 Conjunto",C(45,-155,103),C(0,-3,1),C(-65,-73,-14),C(73,65,31));
  Camera("CAPILLA - 02 Casa y jardin",C(23,-80,35),C(0,-3,1),C(-32,-38,-9),C(30,29,21));
  Camera("CAPILLA - 03 Corredor",C(14,-38,12),C(0,-3,1.5),C(-12,-11,-1.2),C(14,7,15));
  Camera("CAPILLA - 04 Implantacion",C(0,-.1,160),C(0,0,0),C(-65,-73,-14),C(73,65,31));
 });var view=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().First(x=>x.Name=="CAPILLA - 02 Casa y jardin");UI.ActiveUIDocument.ActiveView=view;var uv=UI.ActiveUIDocument.GetOpenUIViews().FirstOrDefault(x=>x.ViewId==view.Id);if(uv!=null)uv.ZoomToFit();}
 static void Export(string which){var views=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().Where(v=>!v.IsTemplate&&v.Name.StartsWith("CAPILLA - ")&&(which=="all"||v.Name.Contains(which))).ToList();foreach(var v in views){var options=new ImageExportOptions{FilePath=Path.Combine(Root,"renders","nuevas_tomas"),ExportRange=ExportRange.SetOfViews,HLRandWFViewsFileType=ImageFileType.PNG,ShadowViewsFileType=ImageFileType.PNG,ImageResolution=ImageResolution.DPI_300,ZoomType=ZoomFitType.FitToPage,PixelSize=2400};options.SetViewsAndSheets(new List<ElementId>{v.Id});D.ExportImage(options);}Notes.Add("Exportadas "+views.Count+" vistas");}
 static void Texture(string mat,string file,double scale){var material=Get<Material>(M(mat).Value);using(var scope=new AppearanceAssetEditScope(D)){var asset=scope.Start(material.AppearanceAssetId);var diff=asset.FindByName("generic_diffuse");if(diff.NumberOfConnectedProperties>0)diff.RemoveConnectedAsset();diff.AddConnectedAsset("UnifiedBitmap");var bitmap=diff.GetSingleConnectedAsset();((AssetPropertyString)bitmap.FindByName("unifiedbitmap_Bitmap")).Value=Path.Combine(Root,"revit/texturas",file);foreach(string dim in new[]{"texture_RealWorldScaleX","texture_RealWorldScaleY"}){var prop=bitmap.FindByName(dim) as AssetPropertyDistance;if(prop!=null)prop.Value=UnitUtils.ConvertFromInternalUnits(scale*U,prop.GetUnitTypeId());}scope.Commit(true);}}
 static void Finishes(){Tx("texturas y correccion de cotas",()=>{
  Texture("Cesped","grass_high_quality_color.jpg",3);Texture("Concreto","concrete_cast-in-place_color.jpg",2);Texture("Estuco","Finishes.Plaster.Stucco.Troweled.Light Yellow.jpg",2);
  var floors=new long[]{154640,154738,154747,154755,154763,154771,154779,154788,154804};var roofs=new long[]{163468,155354,155386,155403,155420,155437,155456,155490,155522};
  for(int i=1;i<Buildings.Length;i++){double z=Base(Buildings[i][0],Buildings[i][1]);for(int k=0;k<4;k++){var w=Get<Wall>(154891+(i-1)*4+k);if(w!=null){w.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).Set(z*U);Mark(w,"Edificio "+i);}}var f=Get<Floor>(floors[i]);if(f!=null)f.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set((z+.05)*U);}
  D.Regenerate();foreach(var fi in new FilteredElementCollector(D).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().Where(f=>f.Host is Wall)){var sill=fi.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM);if(sill!=null&&!sill.IsReadOnly)sill.Set(((Wall)fi.Host).get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble()+(fi.Category.Id.Value==(long)BuiltInCategory.OST_Windows?1:0)*U);}
  foreach(long id in new long[]{154895,154896,154897,154898,154899,154900,154901,154902})ApplyType<WallType>(id,"Capilla HD - madera construcciones laterales","Madera");
  foreach(var v in new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().Where(v=>v.Name.StartsWith("CAPILLA - "))){
   foreach(Category cat in D.Settings.Categories){if(cat.Name.IndexOf("curva",StringComparison.OrdinalIgnoreCase)>=0||cat.Name.IndexOf("contour",StringComparison.OrdinalIgnoreCase)>=0)try{v.SetCategoryHidden(cat.Id,true);}catch{}foreach(Category sub in cat.SubCategories)if(sub.Name.IndexOf("curva",StringComparison.OrdinalIgnoreCase)>=0||sub.Name.IndexOf("contour",StringComparison.OrdinalIgnoreCase)>=0)try{v.SetCategoryHidden(sub.Id,true);}catch{}}
   var box=v.GetSectionBox();var hidden=new List<ElementId>();foreach(var p in new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(p=>p.Category.Id.Value==(long)BuiltInCategory.OST_Planting)){var b=p.get_BoundingBox(null);var center=(b.Min+b.Max)/2;if((center.X<box.Min.X||center.X>box.Max.X||center.Y<box.Min.Y||center.Y>box.Max.Y)&&p.CanBeHidden(v))hidden.Add(p.Id);}if(hidden.Count>0)v.HideElements(hidden);
  }
 });
  var roofIds=new long[]{163468,155354,155386,155403,155420,155437,155456,155490,155522};for(int i=0;i<9;i++){int index=i;try{Tx("encuentros de cubierta "+i,()=>{var ids=index==0?new long[]{154879,154882,154885,154888}:Enumerable.Range(0,4).Select(k=>154891L+(index-1)*4+k).ToArray();foreach(long id in ids){var w=Get<Wall>(id);w.AddAttachment(E(roofIds[index]),AttachmentLocation.Top);}});}catch(Exception ex){Notes.Add("Encuentro "+i+": "+ex.Message);}}
 }
 static void Vehicle(double x,double y,double angle,bool white){var g=new List<GeometryObject>();string body=white?"Blanco":"Metal gris";g.Add(Box(-.88,-2.12,.40,1.76,4.24,.48,body));g.Add(Box(-.82,-1.17,.88,1.64,2.40,.72,"Vidrio"));g.Add(Box(-.87,-1.12,1.55,1.74,2.30,.10,body));g.Add(Box(-.84,-2.02,.88,1.68,.82,.16,body));g.Add(Box(-.9,-2.19,.36,1.8,.11,.19,"Carpinteria"));g.Add(Box(-.9,2.13,.36,1.8,.10,.19,"Carpinteria"));foreach(double xx in new[]{-.9,.9}){foreach(double yy in new[]{-1.27,1.35}){g.Add(Rod(P(xx-.11,yy,.38),P(xx+.11,yy,.38),.34,"Carpinteria",24));g.Add(Rod(P(xx-.12,yy,.38),P(xx+.12,yy,.38),.20,"Metal gris",12));}foreach(double yy in new[]{-1.11,.10,1.14})g.Add(Box(xx-.03,yy,.87,.07,.075,.70,body));}g.Add(Box(-.83,-2.23,.70,.43,.06,.12,"Concreto claro"));g.Add(Box(.40,-2.23,.70,.43,.06,.12,"Concreto claro"));var tr=Transform.CreateRotation(XYZ.BasisZ,angle);tr.Origin=P(x,y,Height(x,y)+.02);var output=g.Cast<Solid>().Select(s=>(GeometryObject)SolidUtils.CreateTransformed(s,tr)).ToList();Shape(white?"Vehiculo blanco de referencia":"Vehiculo gris de referencia",output,BuiltInCategory.OST_Entourage);}
 static void Details(){Tx("detalles de escala y presentacion",()=>{
  Vehicle(21,-7,.4,true);Vehicle(22,-17,-.55,false);
  var extras=new List<GeometryObject>();for(int i=0;i<2;i++){double x=-3.2+i*6.2;extras.Add(Box(x,-6.25,.46,1.3,.45,.07,"Madera"));extras.Add(Box(x,-5.88,.48,1.3,.06,.60,"Madera"));foreach(double xx in new[]{x+.12,x+1.08})extras.Add(Box(xx,-6.20,.04,.075,.30,.43,"Madera"));}Shape("Bancos del corredor",extras);
  var clad=new List<GeometryObject>();double z=Base(-30,6);for(double zz=z+.2;zz<z+2.7;zz+=.19)clad.Add(Box(-35.06,1.89,zz,10.12,.04,.025,"Carpinteria"));Shape("Tablas de fachada de la casa lateral",clad);
  // Preserve the presentation lighting chosen in Revit and apply only graphics settings.
  var source=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().First(v=>v.Name=="CAPILLA - 02 Casa y jardin");var template=source.CreateViewTemplate();template.Name="CAPILLA - Presentacion con sombras";
  var graphics=new HashSet<long>(new[]{BuiltInParameter.GRAPHIC_DISPLAY_OPTIONS_SHADOWS,BuiltInParameter.GRAPHIC_DISPLAY_OPTIONS_LIGHTING,BuiltInParameter.GRAPHIC_DISPLAY_OPTIONS_SS_INTENSITY,BuiltInParameter.GRAPHIC_DISPLAY_OPTIONS_PHOTO_EXPOSURE,BuiltInParameter.GRAPHIC_DISPLAY_OPTIONS_MODEL,BuiltInParameter.GRAPHIC_DISPLAY_OPTIONS_BACKGROUND}.Select(x=>(long)x));template.SetNonControlledTemplateParameterIds(template.GetTemplateParameterIds().Where(id=>!graphics.Contains(id.Value)).ToList());
  foreach(var v in new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().Where(v=>v.Name.StartsWith("CAPILLA - ")&&!v.IsTemplate)){v.ApplyViewTemplateParameters(template);var box=v.GetSectionBox();var hide=new List<ElementId>();foreach(var p in new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(p=>p.Category.Id.Value==(long)BuiltInCategory.OST_Planting)){var b=p.get_BoundingBox(null);var c=(b.Min+b.Max)/2;if((c.X<box.Min.X||c.X>box.Max.X||c.Y<box.Min.Y||c.Y>box.Max.Y)&&p.CanBeHidden(v))hide.Add(p.Id);}if(hide.Count>0)v.HideElements(hide);}
 });}
 static void Polish(){Tx("encuadres finales",()=>{
  var allViews=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().Where(v=>!v.IsTemplate&&v.Name.StartsWith("CAPILLA")).ToList();foreach(var v in allViews){var box=v.GetSectionBox();box.Max=new XYZ(box.Max.X,box.Max.Y,55*U);v.SetSectionBox(box);}
  var full=allViews.First(v=>v.Name=="CAPILLA - 01 Conjunto");full.SetSectionBox(new BoundingBoxXYZ{Min=P(-88,-78,-15),Max=P(90,92,55)});
  var pl=new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(p=>p.Category.Id.Value==(long)BuiltInCategory.OST_Planting).ToList();var restore=pl.Where(p=>p.IsHidden(full)).Select(p=>p.Id).ToList();if(restore.Count>0)full.UnhideElements(restore);
  foreach(var p in pl){var b=p.get_BoundingBox(null);var c=(b.Min+b.Max)/2*.3048;double pond=Math.Pow((c.X-2)/5.7,2)+Math.Pow((c.Y+50)/4,2);if(pond<1&&p.ApplicationDataId.StartsWith("Arbusto"))D.Delete(p.Id);}
  foreach(var v in allViews.Where(v=>v.Id!=full.Id)){var box=v.GetSectionBox();var hide=new List<ElementId>();foreach(var p in pl.Where(p=>p.IsValidObject)){var b=p.get_BoundingBox(null);var c=(b.Min+b.Max)/2;if((c.X<box.Min.X||c.X>box.Max.X||c.Y<box.Min.Y||c.Y>box.Max.Y)&&p.CanBeHidden(v))hide.Add(p.Id);}if(hide.Count>0)v.HideElements(hide);}
  var vt=new FilteredElementCollector(D).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().First(x=>x.ViewFamily==ViewFamily.ThreeDimensional);var persp=View3D.CreatePerspective(D,vt.Id);persp.Name="CAPILLA - 05 Dron";var eye=P(24,-66,27);var f=(P(0,-1,2)-eye).Normalize();var r=f.CrossProduct(XYZ.BasisZ).Normalize();persp.SetOrientation(new ViewOrientation3D(eye,r.CrossProduct(f),f));persp.SetSectionBox(new BoundingBoxXYZ{Min=P(-105,-94,-18),Max=P(105,100,58)});persp.IsSectionBoxActive=true;persp.DetailLevel=ViewDetailLevel.Fine;persp.DisplayStyle=DisplayStyle.Realistic;var template=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().First(v=>v.Name=="CAPILLA - Presentacion con sombras");persp.ApplyViewTemplateParameters(template);foreach(Category cat in D.Settings.Categories){if(cat.Name.IndexOf("curva",StringComparison.OrdinalIgnoreCase)>=0)try{persp.SetCategoryHidden(cat.Id,true);}catch{}foreach(Category sub in cat.SubCategories)if(sub.Name.IndexOf("curva",StringComparison.OrdinalIgnoreCase)>=0)try{persp.SetCategoryHidden(sub.Id,true);}catch{}}foreach(var bic in new[]{BuiltInCategory.OST_Levels,BuiltInCategory.OST_Grids,BuiltInCategory.OST_SectionBox})try{persp.SetCategoryHidden(new ElementId(bic),true);}catch{}
  var comments=D.ProjectInformation.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);if(comments!=null&&!comments.IsReadOnly)comments.Set("Reconstruccion visual exterior a partir de video de dron de 21 s. Escala, cotas, limites y zonas ocultas aproximados. Sin levantamiento topografico ni medidas verificadas. Arquitectura y terreno nativos; detalles y vegetacion mediante DirectShape.");
 });}
 static void Coordination(){var warnings=D.GetWarnings().ToList();int joined=0,excavated=0;foreach(var warning in warnings){var ids=warning.GetFailingElements().ToList();var elems=ids.Select(id=>D.GetElement(id)).ToList();var floors=elems.OfType<Floor>().ToList();var topo=elems.OfType<Toposolid>().FirstOrDefault();try{if(topo!=null&&floors.Count>0){Tx("encuentro pavimento terreno",()=>{foreach(var f in floors)if(topo.CanBeExcavatedBy(f.Id)){topo.ExcavateBy(f.Id);excavated++;}});}else if(floors.Count==2){Tx("union de pavimentos",()=>{if(!JoinGeometryUtils.AreElementsJoined(D,floors[0],floors[1])){JoinGeometryUtils.JoinGeometry(D,floors[0],floors[1]);joined++;}});}}catch(Exception ex){Notes.Add(ex.Message);}}Notes.Add("Uniones "+joined+", ajustes de terreno "+excavated);}
 static object Check(){var all=new FilteredElementCollector(D).WhereElementIsNotElementType().ToElements();return new{path=D.PathName,elements=all.Count,detailElements=all.OfType<DirectShape>().Count(x=>x.ApplicationId==Tag),plants=all.OfType<DirectShape>().Count(x=>x.Category.Id.Value==(long)BuiltInCategory.OST_Planting),warnings=D.GetWarnings().Select(w=>w.GetDescriptionText()).GroupBy(x=>x).Select(g=>new{message=g.Key,count=g.Count()}),walls=all.OfType<Wall>().Select(w=>new{id=w.Id.Value,min=w.get_BoundingBox(null).Min.Z*.3048,max=w.get_BoundingBox(null).Max.Z*.3048}),roofs=all.OfType<RoofBase>().Select(w=>new{id=w.Id.Value,min=w.get_BoundingBox(null).Min.Z*.3048,max=w.get_BoundingBox(null).Max.Z*.3048}),views=all.OfType<View3D>().Where(v=>v.Name.StartsWith("CAPILLA")).Select(v=>new{id=v.Id.Value,name=v.Name})};}
 static void RemoveShape(string name){foreach(var e in new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(e=>e.ApplicationId==Tag&&e.ApplicationDataId==name).ToList())D.Delete(e.Id);}
 static void Baluster(Mesh mesh,double x,double y){double[] zs={.25,.31,.36,.43,.51,.59,.66,.74,.83,.88,.94};double[] rs={.027,.027,.018,.033,.040,.026,.017,.025,.035,.021,.021};for(int i=0;i<zs.Length-1;i++)mesh.Tube(P(x,y,zs[i]),P(x,y,zs[i+1]),rs[i],rs[i+1],"Madera oscura",12);}
 static double CeilingZ(double x,double y){return 2.97+.23*Math.Min(Math.Min(x+10,10-x),Math.Min(y+8.05,5.5-y));}
 static void Walking(){Tx("IMG 9399 - corredor y acceso observados",()=>{
  Mat("Madera oscura",83,49,32);Mat("Madera cielo",159,101,51);Mat("Madera cielo clara",182,125,68);Mat("Terracota baldosa",156,92,57);Mat("Terracota clara",174,108,68);Mat("Junta baldosa",197,177,144);Mat("Ladrillo columna",186,137,94);Mat("Estuco recorrido",231,213,142);Mat("Tapizado oscuro",42,55,48);Mat("Tela ocre",191,147,65);
  foreach(long id in new long[]{154879,154882,154885,154888})ApplyType<WallType>(id,"Capilla recorrido - estuco amarillo","Estuco recorrido");
  foreach(string n in new[]{"Corredor - postes vigas y baranda de madera","Bancos del corredor","9399 Baranda torneada","9399 Madera estructural","9399 Baldosa corredor","9399 Cielo de madera","9399 Carpinterias rejas","9399 Mobiliario corredor","9399 Escalinata acceso"})RemoveShape(n);
  var structure=new List<GeometryObject>();
  foreach(double x in new[]{-9d,-6,-3,-1,1,3,6,9}){structure.Add(Box(x-.065,-7.45,.035,.13,.13,CeilingZ(x,-7.4)-.035,"Madera oscura"));structure.Add(Box(x-.12,-7.51,.035,.24,.25,.16,"Zocalo"));}
  structure.Add(Box(-9.1,-7.46,2.89,18.2,.18,.22,"Madera oscura"));
  for(double x=-8.9;x<9;x+=1.5)structure.Add(Rod(P(x,-7.40,CeilingZ(x,-7.40)-.09),P(x,-4.4,CeilingZ(x,-4.4)-.09),.075,"Madera oscura",4));
  foreach(double y in new[]{-4.4,-1.4,1.6,4.4})structure.Add(Box(9.0,y,.035,.14,.14,CeilingZ(9,y)-.035,"Madera oscura"));
  Shape("9399 Madera estructural",structure);
  var rails=new List<GeometryObject>();var bal=new Mesh();
  foreach(int side in new[]{-1,1}){double a=side<0?-9:1,b=side<0?-1:9;rails.Add(Box(a,-7.4,.96,b-a,.11,.085,"Madera oscura"));rails.Add(Box(a,-7.385,.21,b-a,.08,.075,"Madera oscura"));for(double x=a+.13;x<b;x+=.18)Baluster(bal,x,-7.34);rails.Add(Box(side*9-.045,-7.35,.96,.09,2.9,.085,"Madera oscura"));rails.Add(Box(side*9-.04,-7.35,.21,.08,2.9,.075,"Madera oscura"));for(double y=-7.2;y<-4.5;y+=.18)Baluster(bal,side*9,y);}
  rails.AddRange(bal.Build());Shape("9399 Baranda torneada",rails);
  // Modelled joint pattern follows the visible rectangular terracotta floor; dimensions remain estimated.
  var tiles=new List<GeometryObject>();tiles.Add(Box(-9,-7.32,.052,18,2.88,.012,"Junta baldosa"));int row=0;
  for(double y=-7.31;y<-4.45;y+=.24,row++){for(double x=-8.99;x<8.99;x+=.48){double xx=x+(row%2)*.24;if(xx>=8.99)continue;tiles.Add(Box(xx,y,.064,Math.Min(.472,8.99-xx),Math.Min(.232,-4.44-y),.014,(row+(int)((x+9)/.48))%4==0?"Terracota clara":"Terracota baldosa"));}}
  Shape("9399 Baldosa corredor",tiles);
  var ceiling=new Mesh();int board=0;for(double y=-7.7;y<-4.40;y+=.14,board++){for(double x=-9.3;x<9.29;x+=.5){double x2=Math.Min(x+.497,9.3),y2=Math.Min(y+.134,-4.4);ceiling.Quad(P(x,y,CeilingZ(x,y)),P(x2,y,CeilingZ(x2,y)),P(x2,y2,CeilingZ(x2,y2)),P(x,y2,CeilingZ(x,y2)),board%5==0?"Madera cielo clara":"Madera cielo");}}Shape("9399 Cielo de madera",ceiling.Build());
  var frames=new List<GeometryObject>();foreach(double x in new[]{-5.4,-2.7,3.24,5.76}){for(double xx=x-.40;xx<x+.45;xx+=.18)frames.Add(Rod(P(xx,-4.73,1.03),P(xx,-4.73,2.21),.011,"Carpinteria",8));foreach(double z in new[]{1.33,1.94})frames.Add(Rod(P(x-.48,-4.73,z),P(x+.48,-4.73,z),.012,"Carpinteria",8));}Shape("9399 Carpinterias rejas",frames);
  var furniture=new List<GeometryObject>();furniture.Add(Box(-7.5,-5.5,.20,1.85,.75,.28,"Tapizado oscuro"));furniture.Add(Box(-7.5,-4.88,.48,1.85,.15,.48,"Tapizado oscuro"));foreach(double xx in new[]{-7.5,-5.80})furniture.Add(Box(xx,-5.5,.45,.15,.72,.23,"Tapizado oscuro"));furniture.Add(Box(-7.15,-6.55,.38,1.1,.65,.06,"Madera cielo"));foreach(double xx in new[]{-7.10,-6.16})foreach(double yy in new[]{-6.5,-6.0})furniture.Add(Box(xx,yy,.04,.06,.06,.34,"Madera oscura"));
  var hammock=new Mesh();for(int i=0;i<24;i++){double t0=(double)i/24,t1=(double)(i+1)/24,x0=3.2+2.5*t0,x1=3.2+2.5*t1,z0=1.2-.65*Math.Sin(Math.PI*t0),z1=1.2-.65*Math.Sin(Math.PI*t1),w0=.50*Math.Sin(Math.PI*t0),w1=.50*Math.Sin(Math.PI*t1);hammock.Quad(P(x0,-5.9-w0,z0),P(x1,-5.9-w1,z1),P(x1,-5.9+w1,z1),P(x0,-5.9+w0,z0),i%3==0?"Terracota baldosa":"Tela ocre");}furniture.AddRange(hammock.Build());Shape("9399 Mobiliario corredor",furniture,BuiltInCategory.OST_Furniture);
  foreach(var f in new FilteredElementCollector(D).OfClass(typeof(Floor)).Cast<Floor>().Where(f=>(f.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString()??"").Contains("Acceso central")).ToList())D.Delete(f.Id);
  var stairs=new List<GeometryObject>();var points=Sample(new[]{new[]{-12d,-30},new[]{-5d,-16},new[]{0d,-7.55}},.70);
  for(int i=0;i<points.Count-1;i++){var a=points[i];var b=points[i+1];double dx=b[0]-a[0],dy=b[1]-a[1],len=Math.Sqrt(dx*dx+dy*dy);double z=Math.Max(Height(a[0],a[1]),Height(b[0],b[1]));double top=Math.Ceiling((z+.12)/.15)*.15;top=Math.Min(top,.035);var solid=Box(-.75,0,top-.22,1.5,len+.009,.22,"Concreto");var tr=Transform.CreateRotation(XYZ.BasisZ,Math.Atan2(-dx,dy));tr.Origin=P(a[0],a[1],0);stairs.Add(SolidUtils.CreateTransformed(solid,tr));}
  Shape("9399 Escalinata acceso",stairs);
  RemoveShape("Pino 1400");Tree("Pino",8,-10.5,13.8,2.2,1400);
  var comments=D.ProjectInformation.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);if(comments!=null&&!comments.IsReadOnly)comments.Set("Referencias: dron 21 s e IMG_9399.MOV 11:05. Corredor, acabados y acceso revisados por observacion. Dimensiones y emplazamientos estimados; interiores completos no confirmados. Detalles DirectShape; estructura base nativa Revit.");
 });}
 static void WalkingFix(){Tx("IMG 9399 - encuentros y apoyos",()=>{
  var triangles=new List<XYZ[]>();var topo=new FilteredElementCollector(D).OfClass(typeof(Toposolid)).First();foreach(var solid in topo.get_Geometry(new Options()).OfType<Solid>())foreach(Face face in solid.Faces){var mesh=face.Triangulate();for(int i=0;i<mesh.NumTriangles;i++){var tri=mesh.get_Triangle(i);triangles.Add(new[]{tri.get_Vertex(0)*.3048,tri.get_Vertex(1)*.3048,tri.get_Vertex(2)*.3048});}}
  Func<double,double,double> ground=(x,y)=>{double best=-100;foreach(var tri in triangles){var a=tri[0];var b=tri[1];var c=tri[2];double det=(b.Y-c.Y)*(a.X-c.X)+(c.X-b.X)*(a.Y-c.Y);if(Math.Abs(det)<1e-9)continue;double u=((b.Y-c.Y)*(x-c.X)+(c.X-b.X)*(y-c.Y))/det,v=((c.Y-a.Y)*(x-c.X)+(a.X-c.X)*(y-c.Y))/det,w=1-u-v;if(u>=-1e-6&&v>=-1e-6&&w>=-1e-6)best=Math.Max(best,u*a.Z+v*b.Z+w*c.Z);}return best;};
  RemoveShape("9399 Escalinata acceso");var stairs=new List<GeometryObject>();var points=Sample(new[]{new[]{-12d,-30},new[]{-5d,-16},new[]{0d,-7.55}},.70);
  for(int i=0;i<points.Count-1;i++){var a=points[i];var b=points[i+1];double dx=b[0]-a[0],dy=b[1]-a[1],len=Math.Sqrt(dx*dx+dy*dy),nx=dy/len*.75,ny=-dx/len*.75;double z=new[]{ground(a[0]+nx,a[1]+ny),ground(a[0]-nx,a[1]-ny),ground(b[0]+nx,b[1]+ny),ground(b[0]-nx,b[1]-ny)}.Max();double top=Math.Min(.078,Math.Ceiling((z+.04)/.15)*.15);var solid=Box(-.75,0,top-.40,1.5,len+.015,.40,"Concreto");var tr=Transform.CreateRotation(XYZ.BasisZ,Math.Atan2(-dx,dy));tr.Origin=P(a[0],a[1],0);stairs.Add(SolidUtils.CreateTransformed(solid,tr));}Shape("9399 Escalinata acceso",stairs);
  RemoveShape("9399 Apoyos hamaca");Shape("9399 Apoyos hamaca",new List<GeometryObject>{Rod(P(3.2,-5.9,1.2),P(3,-7.4,2.1),.009,"Tela ocre",8),Rod(P(5.7,-5.9,1.2),P(6,-7.4,2.1),.009,"Tela ocre",8)});
 });}
 static void WalkingViews(){Tx("IMG 9399 - vistas del recorrido",()=>{
  var vt=new FilteredElementCollector(D).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().First(x=>x.ViewFamily==ViewFamily.ThreeDimensional);
  var template=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().First(v=>v.Name=="CAPILLA - Presentacion con sombras");
  var setups=new[]{new {Name="CAPILLA - 06 Acceso recorrido",Eye=P(-15,-36,3),Target=P(0,-5,1.8)},new{Name="CAPILLA - 07 Corredor recorrido",Eye=P(8,-7.0,1.7),Target=P(-7,-5.5,1.6)}};
  foreach(var s in setups){var v=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().FirstOrDefault(x=>x.Name==s.Name)??View3D.CreatePerspective(D,vt.Id);v.Name=s.Name;var f=(s.Target-s.Eye).Normalize();var r=f.CrossProduct(XYZ.BasisZ).Normalize();v.SetOrientation(new ViewOrientation3D(s.Eye,r.CrossProduct(f),f));v.SetSectionBox(new BoundingBoxXYZ{Min=P(-50,-50,-15),Max=P(60,50,55)});v.IsSectionBoxActive=true;v.DetailLevel=ViewDetailLevel.Fine;v.DisplayStyle=DisplayStyle.Realistic;v.ApplyViewTemplateParameters(template);foreach(var bic in new[]{BuiltInCategory.OST_Levels,BuiltInCategory.OST_Grids,BuiltInCategory.OST_SectionBox})try{v.SetCategoryHidden(new ElementId(bic),true);}catch{}foreach(Category cat in D.Settings.Categories){foreach(Category sub in cat.SubCategories)if(sub.Name.IndexOf("curva",StringComparison.OrdinalIgnoreCase)>=0)try{v.SetCategoryHidden(sub.Id,true);}catch{}}}
 });}


 static void SiteMat(string n,byte r,byte g,byte b){if(!Mats.ContainsKey(n))Mat(n,r,g,b);}
 static string Evidence="IMG_9399.MOV; identificacion contrastada con usuario; cotas y posiciones estimadas";
 static void SiteMark(Element e,string n){e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.Set("SITIO_9399 - "+n+" - "+Evidence);}
 static Floor SiteSlab(string n,IList<XYZ> pts,double z,string mat){var name="Capilla sitio - suelo "+mat;var ft=new FilteredElementCollector(D).OfClass(typeof(FloorType)).Cast<FloorType>().FirstOrDefault(t=>t.Name==name);if(ft==null){ft=(FloorType)Get<FloorType>(1238).Duplicate(name);var cs=ft.GetCompoundStructure();for(int i=0;i<cs.LayerCount;i++)cs.SetMaterialId(i,M(mat));ft.SetCompoundStructure(cs);}var f=Floor.Create(D,new List<CurveLoop>{Loop(pts)},ft.Id,L.Id);f.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(z*U);SiteMark(f,n);return f;}
 static FootPrintRoof SiteRoof(string n,double x,double y,double w,double d,double z,double slope,bool hip,string mat){var name="Capilla sitio - cubierta "+mat;var rt=new FilteredElementCollector(D).OfClass(typeof(RoofType)).Cast<RoofType>().FirstOrDefault(t=>t.Name==name);if(rt==null){rt=(RoofType)Get<RoofType>(1400).Duplicate(name);var cs=rt.GetCompoundStructure();for(int i=0;i<cs.LayerCount;i++)cs.SetMaterialId(i,M(mat));rt.SetCompoundStructure(cs);}var ps=new[]{P(x-w/2,y-d/2),P(x+w/2,y-d/2),P(x+w/2,y+d/2),P(x-w/2,y+d/2)};var ca=new CurveArray();for(int i=0;i<4;i++)ca.Append(Line.CreateBound(ps[i],ps[(i+1)%4]));var curves=new ModelCurveArray();var roof=D.Create.NewFootPrintRoof(ca,L,rt,out curves);roof.get_Parameter(BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM).Set(z*U);int k=0;foreach(ModelCurve mc in curves){roof.set_DefinesSlope(mc,hip||k%2==((n.StartsWith("Gallinero")||n.StartsWith("Portada")||n.StartsWith("Cocina triangular"))?1:0));if(hip||k%2==((n.StartsWith("Gallinero")||n.StartsWith("Portada")||n.StartsWith("Cocina triangular"))?1:0))roof.set_SlopeAngle(mc,slope);k++;}SiteMark(roof,n);return roof;}
 static void SiteCleanup(){foreach(var e in new FilteredElementCollector(D).WhereElementIsNotElementType().Where(e=>(e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString()??"").StartsWith("SITIO_9399")).ToList())if(e.IsValidObject)D.Delete(e.Id);foreach(var e in new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(e=>e.ApplicationDataId.StartsWith("SITIO - ")).ToList())D.Delete(e.Id);}
 static List<XYZ> Rectangle(double x,double y,double w,double d){return new List<XYZ>{P(x-w/2,y-d/2),P(x+w/2,y-d/2),P(x+w/2,y+d/2),P(x-w/2,y+d/2)};}
 static double[][] Lakes={new[]{9d,26,5.0,4.5},new[]{7d,40,3.4,5.0},new[]{2d,-50,5.6,3.8}};
 static double[] LakePoint(int j,double t,double scale=1){var q=Lakes[j];double xx,yy;if(j==2){xx=Math.Sign(Math.Cos(t))*Math.Pow(Math.Abs(Math.Cos(t)),.45);yy=Math.Sign(Math.Sin(t))*Math.Pow(Math.Abs(Math.Sin(t)),.45);}else{double r=j==1?1+.18*Math.Cos(3*t):1;xx=r*Math.Cos(t);yy=r*Math.Sin(t);}return new[]{q[0]+q[2]*xx*scale,q[1]+q[3]*yy*scale};}
 static void SiteBuildings(){Tx("Recorrido completo - tanques gallinero cabana y kiosco",()=>{
  SiteMat("Plastico negro",34,38,35);SiteMat("Malla metalica",104,114,107);SiteMat("Verde gallinero",65,101,71);SiteMat("Rojo corredor",145,67,49);SiteMat("Borde lago",180,161,116);
  SiteCleanup();
  // Replace generic volumes only where the video now identifies their function and form.
  foreach(long id in new long[]{154771,155437,154907,154908,154909,154910,154788,155490,154915,154916,154917,154918,154804,155522,154919,154920,154921,154922})Del(id);
  RemoveShape("Plataforma y cerca del santuario");RemoveShape("Jardineras y deposito circular");
  double z=Base(55,50);SiteSlab("Gallinero - solera",Rectangle(55,50,5.8,14),z+.04,"Concreto");SiteRoof("Gallinero - cubierta alargada a dos aguas",55,50,6.8,15.0,z+2.15,.18,false,"Verde gallinero");
  var coop=new List<GeometryObject>();coop.Add(Box(52.1,43,z,5.8,.12,.45,"Blanco"));coop.Add(Box(52.1,56.9,z,5.8,.12,.45,"Blanco"));foreach(double x in new[]{52.1,57.8}){coop.Add(Box(x,43,z,.12,14,.45,"Blanco"));for(double y=43;y<=57;y+=2)coop.Add(Rod(P(x+.06,y,z+.4),P(x+.06,y,z+2.18),.06,"Tronco",10));}
  for(double y=43;y<57;y+=.17){foreach(double x in new[]{52.15,57.85})coop.Add(Rod(P(x,y,z+.45),P(x,y,z+2.10),.0025,"Malla metalica",4));}for(double h=.5;h<2.1;h+=.17)foreach(double x in new[]{52.15,57.85})coop.Add(Rod(P(x,43,z+h),P(x,57,z+h),.0025,"Malla metalica",4));
  for(double x=52.2;x<57.8;x+=.17)coop.Add(Rod(P(x,43,z+.45),P(x,43,z+2.1),.0025,"Malla metalica",4));for(double h=.5;h<2.1;h+=.17)coop.Add(Rod(P(52.1,43,z+h),P(57.9,43,z+h),.0025,"Malla metalica",4));
  for(double y=43;y<=57;y+=2)coop.Add(Rod(P(52.1,y,z+2.1),P(57.9,y,z+2.1),.05,"Tronco",8));Shape("SITIO - Gallinero malla y postes 00m50",coop);
  double tz=Base(58,64);SiteSlab("Tanques - plataforma alta",Rectangle(58,64,6.4,4.8),tz+.05,"Concreto");var tanks=new List<GeometryObject>();foreach(var xy in new[]{new[]{55.9,63.0},new[]{58d,62.8},new[]{60.2,64.5}}){tanks.Add(Rod(P(xy[0],xy[1],tz+.06),P(xy[0],xy[1],tz+1.42),.72,"Plastico negro",48));tanks.Add(Rod(P(xy[0],xy[1],tz+1.42),P(xy[0],xy[1],tz+1.51),.77,"Plastico negro",48));tanks.Add(Rod(P(xy[0],xy[1],tz+1.51),P(xy[0],xy[1],tz+1.55),.24,"Plastico negro",32));tanks.Add(Rod(P(xy[0]+.7,xy[1],tz+.2),P(xy[0]+.9,xy[1],tz+.2),.025,"Blanco",8));}
  tanks.Add(Rod(P(57.5,65.1,tz+.05),P(57.5,65.1,tz+1.45),1.02,"Concreto",48));tanks.Add(Rod(P(55.4,62.1,tz+.10),P(61.0,62.1,tz+.10),.024,"Blanco",8));foreach(double x in new[]{56.6,58.7,60.9})tanks.Add(Rod(P(x,62.1,tz+.1),P(x,64.5,tz+.1),.024,"Blanco",8));Shape("SITIO - Tanques de agua al inicio 00m00",tanks);
  // Small white cabin: keep its native walls and openings, extend the green roof over the red veranda.
  foreach(long id in new long[]{154891,154892,154893,154894}){var w=Get<Wall>(id);if(w!=null)foreach(var a in w.GetAttachmentIds(AttachmentLocation.Top).ToList())w.RemoveAttachment(a);}Del(155354);double cz=Base(48,18);var cabinRoof=SiteRoof("Cabana pequena - cubierta verde",48,17.35,11.7,9.0,cz+2.8,.22,false,"Verde gallinero");foreach(long id in new long[]{154891,154892,154893,154894}){var w=Get<Wall>(id);if(w!=null){w.AddAttachment(cabinRoof.Id,AttachmentLocation.Top);SiteMark(w,"Cabana pequena - muro existente");}}
  SiteSlab("Cabana pequena - corredor rojo",Rectangle(48,14.25,10.1,1.5),cz+.055,"Rojo corredor");var cabin=new List<GeometryObject>();foreach(double x in new[]{43d,45.5,48,50.5,53})cabin.Add(Box(x-.045,13.45,cz+.055,.09,.09,2.75,"Verde gallinero"));foreach(var seg in new[]{new[]{43d,46.8},new[]{48.1,53d}}){cabin.Add(Rod(P(seg[0],13.5,cz+1),P(seg[1],13.5,cz+1),.027,"Verde gallinero",8));for(double x=seg[0];x<seg[1];x+=.18)cabin.Add(Rod(P(x,13.5,cz+.09),P(x,13.5,cz+1),.013,"Verde gallinero",8));}cabin.Add(Rod(P(42.5,13.1,cz+2.75),P(53.5,13.1,cz+2.75),.065,"Blanco",12));Shape("SITIO - Cabana pequena baranda y canalon 02m40",cabin);
  // Small red-roof dog house replaces the earlier sanctuary interpretation.
  double dz=Base(26,30);SiteSlab("Casa del perro - plataforma",Rectangle(26,30,4.5,3.7),dz+.03,"Concreto claro");var dog=new List<GeometryObject>();dog.Add(Box(25.3,29.25,dz+.03,.12,1.5,1.05,"Blanco"));dog.Add(Box(26.58,29.25,dz+.03,.12,1.5,1.05,"Blanco"));dog.Add(Box(25.3,30.63,dz+.03,1.4,.12,1.05,"Blanco"));dog.Add(Box(25.3,29.25,dz+.03,.39,.12,1.05,"Blanco"));dog.Add(Box(26.31,29.25,dz+.03,.39,.12,1.05,"Blanco"));dog.Add(Box(25.69,29.25,dz+.86,.62,.12,.22,"Blanco"));SiteRoof("Casa del perro - tejado rojo",26,30,1.85,1.98,dz+1.05,.65,false,"Teja");
  foreach(double y in new[]{28.3,31.7}){for(double x=23.9;x<=28.1;x+=.85)dog.Add(Rod(P(x,y,dz),P(x,y,dz+.78),.04,"Blanco",6));foreach(double zz in new[]{.32,.7})dog.Add(Rod(P(23.9,y,dz+zz),P(28.1,y,dz+zz),.028,"Blanco",6));}foreach(double x in new[]{23.9,28.1})foreach(double zz in new[]{.32,.7})dog.Add(Rod(P(x,28.3,dz+zz),P(x,31.7,dz+zz),.028,"Blanco",6));Shape("SITIO - Casa del perro y cerca blanca",dog);
  // Low kiosk: octagonal, open sided, with masonry benches and a green pyramidal roof.
  double kx=-24,ky=-60,kz=Base(kx,ky);var oct=Enumerable.Range(0,8).Select(i=>P(kx+5.2*Math.Cos(Math.PI/8+i*Math.PI/4),ky+5.2*Math.Sin(Math.PI/8+i*Math.PI/4))).ToList();SiteSlab("Kiosco inferior - piso octagonal",oct,kz+.07,"Rojo corredor");var kiosk=new List<GeometryObject>();var km=new Mesh();for(int i=0;i<8;i++){double a=Math.PI/8+i*Math.PI/4,b=a+Math.PI/4;var p=P(kx+5*Math.Cos(a),ky+5*Math.Sin(a),kz+.07);kiosk.Add(Rod(p,p+P(0,0,2.55),.075,"Madera oscura",8));kiosk.Add(Rod(p,p+P(0,0,.5),.11,"Blanco",8));var e=P(kx+5.8*Math.Cos(a),ky+5.8*Math.Sin(a),kz+2.52);var f=P(kx+5.8*Math.Cos(b),ky+5.8*Math.Sin(b),kz+2.52);km.Tri(e,f,P(kx,ky,kz+4.18),"Verde gallinero");kiosk.Add(Rod(e,f,.07,"Madera oscura",8));kiosk.Add(Rod(e,P(kx,ky,kz+4.18),.028,"Techo verde claro",6));if(i!=1&&i!=5){var end=P(kx+4.7*Math.Cos(b),ky+4.7*Math.Sin(b),kz+.47);var start=P(kx+4.7*Math.Cos(a),ky+4.7*Math.Sin(a),kz+.47);kiosk.Add(Rod(start,end,.16,"Blanco",4));}}
  kiosk.AddRange(km.Build());Shape("SITIO - Kiosco inferior abierto 07m50",kiosk);
  var dry=new Mesh();double yz=Height(-15,-48);for(int i=0;i<72;i++){double a=i*Math.PI/36,b=(i+1)*Math.PI/36;dry.Tri(P(-15,-48,yz+.05),P(-15+2.8*Math.Cos(a),-48+2.8*Math.Sin(a),yz+.05),P(-15+2.8*Math.Cos(b),-48+2.8*Math.Sin(b),yz+.05),"Concreto claro");}Shape("SITIO - Plataforma circular seca junto al kiosco",dry.Build());
 });}
 static double SiteHeight(double x,double y){double z=Height(x,y);foreach(var pad in new[]{new[]{55d,50,6.6,15.0,Base(55,50)-.1},new[]{58d,64,7.2,5.6,Base(58,64)-.1},new[]{-24d,-60,11d,11d,Base(-24,-60)-.1},new[]{-15d,-48,6.5,6.5,Height(-15,-48)-.1}}){double dx=Math.Max(Math.Abs(x-pad[0])-pad[2]/2,0),dy=Math.Max(Math.Abs(y-pad[1])-pad[3]/2,0),d=Math.Sqrt(dx*dx+dy*dy);if(d<3){double t=d/3;t=t*t*(3-2*t);z=pad[4]*(1-t)+z*t;}}return z;}
 static void SitePolish(){Tx("Recorrido completo - agua senderos y representacion",()=>{
  SiteMat("Agua sitio opaca",53,81,72);for(int j=0;j<3;j++){string n="SITIO - Agua visible lago "+(j+1);RemoveShape(n);var q=Lakes[j];double z=Base(q[0],q[1])-.4;var pts=Enumerable.Range(0,96).Select(i=>{var p=LakePoint(j,i*Math.PI/48);return P(p[0],p[1],z-.015);}).ToList();var solid=GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>{Loop(pts)},XYZ.BasisZ,.03*U,new SolidOptions(M("Agua sitio opaca"),ElementId.InvalidElementId));Shape(n,new List<GeometryObject>{solid});}
  Del(164592);foreach(var f in new FilteredElementCollector(D).OfClass(typeof(Floor)).Where(f=>(f.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString()??"").Contains("Sendero posterior revisado")).ToList())D.Delete(f.Id);
  var upper=Sample(new[]{new[]{8d,6},new[]{16d,15},new[]{18d,24},new[]{18d,34},new[]{13d,44}},1.2);Pavement("Sendero posterior revisado",upper,1.4,"Concreto claro",.16);
  RemoveShape("Bordillos y juntas de pavimentos");var borders=new Mesh();var central=Sample(new[]{new[]{0d,-7.55},new[]{-5d,-16},new[]{-12d,-30}},1.2);var road=Sample(new[]{new[]{-47d,-36.4},new[]{-25d,-32},new[]{-12d,-30},new[]{5d,-27},new[]{15d,-24}},1.8);var lower=Sample(new[]{new[]{-12d,-30},new[]{-13.5,-34},new[]{-17d,-38},new[]{-18d,-42},new[]{-15d,-45}},1.2);foreach(var line in new[]{central,road,upper,lower}){double w=line==road?3.55:line==central?1.5:line==upper?1.4:1.1;Ribbon(borders,Offset(line,w/2+.035),.085,"Blanco",.22);Ribbon(borders,Offset(line,-w/2-.035),.085,"Blanco",.22);}Shape("Bordillos y juntas de pavimentos",borders.Build());
  RemoveShape("Borduras vegetales del recorrido principal");var hedges=new Mesh();foreach(double side in new[]{-1d,1d}){foreach(var p in Offset(central,side*1.03))Tuft(hedges,p[0],p[1],1,"Borde vegetal");foreach(var p in Offset(upper,side*.98))Tuft(hedges,p[0],p[1],1,"Borde vegetal");}Shape("Borduras vegetales del recorrido principal",hedges.Build(),BuiltInCategory.OST_Planting);
  foreach(var v in new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().Where(v=>!v.IsTemplate&&v.Name.StartsWith("CAPILLA"))){foreach(Category cat in D.Settings.Categories){if(cat.Name.IndexOf("curva",StringComparison.OrdinalIgnoreCase)>=0||cat.Name.IndexOf("contour",StringComparison.OrdinalIgnoreCase)>=0)try{v.SetCategoryHidden(cat.Id,true);}catch{}foreach(Category sub in cat.SubCategories)if(sub.Name.IndexOf("curva",StringComparison.OrdinalIgnoreCase)>=0||sub.Name.IndexOf("contour",StringComparison.OrdinalIgnoreCase)>=0)try{v.SetCategoryHidden(sub.Id,true);}catch{}}}
 });}
 static void SiteLakes(){Tx("Recorrido completo - tres lagos diferenciados",()=>{
  foreach(string n in new[]{"Estanque - lamina de agua","SITIO - Lago 1 superior oval","SITIO - Lago 2 superior curvo","SITIO - Lago 3 inferior junto kiosco"})RemoveShape(n);
  var oldTopo=new FilteredElementCollector(D).OfClass(typeof(Toposolid)).Cast<Toposolid>().First();var type=oldTopo.GetTypeId();D.Delete(oldTopo.Id);var points=new Dictionary<string,XYZ>();Action<double,double> add=(x,y)=>{double z=SiteHeight(x,y);for(int j=0;j<3;j++){var q=Lakes[j];double xx=(x-q[0])/q[2],yy=(y-q[1])/q[3];double r=j==2?Math.Pow(Math.Pow(Math.Abs(xx),4.4)+Math.Pow(Math.Abs(yy),4.4),1/4.4):Math.Sqrt(xx*xx+yy*yy);double level=Base(q[0],q[1])-.40;if(r<1.45){double t=Math.Max(0,Math.Min(1,(r-.9)/.55));t=t*t*(3-2*t);z=(level-1)*(1-t)+z*t;}}points[x.ToString("F3")+","+y.ToString("F3")]=P(x,y,z);};for(double x=-110;x<=110;x+=5)for(double y=-95;y<=105;y+=5)add(x,y);for(double x=50;x<=64;x+=1)for(double y=40;y<=69;y+=1)add(x,y);for(double x=-31;x<=-10;x+=1)for(double y=-67;y<=-43;y+=1)add(x,y);for(double x=-16;x<=16;x+=2)for(double y=-12;y<=10;y+=2)add(x,y);foreach(var q in Lakes)for(double x=q[0]-q[2]*1.6;x<=q[0]+q[2]*1.6;x+=.65)for(double y=q[1]-q[3]*1.6;y<=q[1]+q[3]*1.6;y+=.65)add(x,y);var topo=Toposolid.Create(D,points.Values.ToList(),type,L.Id);Mark(topo,"Terreno con tres vasos aproximados del recorrido");
  for(int j=0;j<3;j++){var q=Lakes[j];double z=Base(q[0],q[1])-.4;var mesh=new Mesh();for(int i=0;i<96;i++){double a=i*Math.PI/48,b=(i+1)*Math.PI/48;var p=LakePoint(j,a);var p2=LakePoint(j,b);var o=LakePoint(j,a,1.045);var o2=LakePoint(j,b,1.045);mesh.Tri(P(q[0],q[1],z),P(p[0],p[1],z),P(p2[0],p2[1],z),"Agua");mesh.Quad(P(p[0],p[1],z+.11),P(p2[0],p2[1],z+.11),P(o2[0],o2[1],z+.11),P(o[0],o[1],z+.11),"Borde lago");mesh.Quad(P(o[0],o[1],z-.7),P(o2[0],o2[1],z-.7),P(o2[0],o2[1],z+.11),P(o[0],o[1],z+.11),"Piedra oscura");}Shape(j==0?"SITIO - Lago 1 superior oval":j==1?"SITIO - Lago 2 superior curvo":"SITIO - Lago 3 inferior junto kiosco",mesh.Build());}
  foreach(var p in new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(p=>p.Category.Id.Value==(long)BuiltInCategory.OST_Planting).ToList()){var box=p.get_BoundingBox(null);var c=(box.Min+box.Max)/2*.3048;if(Lakes.Any(q=>Math.Pow((c.X-q[0])/(q[2]+1),2)+Math.Pow((c.Y-q[1])/(q[3]+1),2)<1))D.Delete(p.Id);}
 });}
 static void SiteShed(){Tx("Recorrido completo - cobertizo junto a lagos",()=>{
  foreach(long id in new long[]{154903,154904,154905,154906,155420})Del(id);double z=Base(14,46);SiteRoof("Cobertizo superior - cubierta verde pronunciada",14,46,8.0,5.5,z+2.35,.66,false,"Verde gallinero");var geo=new List<GeometryObject>();foreach(double x in new[]{10.7,17.3})foreach(double y in new[]{43.8,48.2})geo.Add(Rod(P(x,y,z),P(x,y,z+2.5),.07,"Tronco",8));geo.Add(Box(10.5,47.9,z,7,.12,1.95,"Blanco"));foreach(double x in new[]{10.55,12.8,15.05}){geo.Add(Box(x,47.73,z+.15,1.6,.13,1.7,"Verde gallinero"));geo.Add(Box(x,47.71,z+1.85,1.65,.17,.12,"Blanco"));}for(double zz=.3;zz<=1.1;zz+=.24){geo.Add(Box(10.63,44,z+zz,.10,3.9,.11,"Madera"));geo.Add(Box(17.25,44,z+zz,.10,3.9,.11,"Madera"));}Shape("SITIO - Cobertizo superior junto a lagos 04m22",geo);
  var seams=new List<GeometryObject>();for(int i=0;i<8;i++){double a=Math.PI/8+i*Math.PI/4,b=a+Math.PI/4;var e=P(-24+5.8*Math.Cos(a),-60+5.8*Math.Sin(a),Base(-24,-60)+2.54);var f=P(-24+5.8*Math.Cos(b),-60+5.8*Math.Sin(b),Base(-24,-60)+2.54);var tip=P(-24,-60,Base(-24,-60)+4.20);for(int k=1;k<18;k++){var p=e+(f-e)*(k/18d);seams.Add(Rod(p,p+(tip-p)*.95,.009,"Juntas cubierta",4));}}Shape("SITIO - Nervaduras cubierta kiosco",seams);
 });}
 static void SiteViews(){Tx("Recorrido completo - vistas verificables",()=>{
  Camera("CAPILLA - 08 Tanques gallinero cabana",C(95,-8,56),C(53,42,7),C(35,9,-1),C(70,71,27));
  Camera("CAPILLA - 09 Lagos superiores y caseta",C(47,-18,39),C(13,29,4),C(-5,13,-1),C(35,53,20));
  Camera("CAPILLA - 10 Lago y kiosco inferior",C(18,-94,27),C(-11,-52,-5),C(-33,-68,-13),C(13,-38,9));
  var template=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().First(v=>v.Name=="CAPILLA - Presentacion con sombras");foreach(var v in new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().Where(v=>!v.IsTemplate&&(v.Name.StartsWith("CAPILLA - 08")||v.Name.StartsWith("CAPILLA - 09")||v.Name.StartsWith("CAPILLA - 10")))){v.ApplyViewTemplateParameters(template);var box=v.GetSectionBox();var ids=new List<ElementId>();foreach(var p in new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(p=>p.Category.Id.Value==(long)BuiltInCategory.OST_Planting)){var b=p.get_BoundingBox(null);var c=(b.Min+b.Max)/2;if(c.X<box.Min.X||c.X>box.Max.X||c.Y<box.Min.Y||c.Y>box.Max.Y)ids.Add(p.Id);}if(ids.Count>0)v.HideElements(ids);}
 });}



 static void SitePortal(){Tx("Portada de acceso segun recorrido 08m02",()=>{
  foreach(long id in new long[]{154779,154911,154912,154913,154914,155456})Del(id);
  foreach(var e in new FilteredElementCollector(D).WhereElementIsNotElementType().Where(e=>(e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString()??"").StartsWith("SITIO_9399 - Portada")).ToList())D.Delete(e.Id);
  RemoveShape("SITIO - Portada pilares y muros");RemoveShape("SITIO - Portada tejas y cerchas");
  SiteMat("Ladrillo portada",175,145,110);SiteMat("Junta portada",214,202,176);
  double x=-38,y=-34.6,z=Height(x,y)+.16;
  SiteRoof("Portada - tejado sobre paso vehicular",x,y,2.9,6.6,z+3.66,.30,false,"Teja");
  var walls=new List<GeometryObject>();var detail=new List<GeometryObject>();
  foreach(double cy in new[]{y-2.5,y+2.5}){
   walls.Add(Box(x-.34,cy-.34,z-.10,.68,.68,3.38,"Ladrillo portada"));
   walls.Add(Box(x-.40,cy-.40,z-.10,.8,.8,.16,"Concreto"));
   for(int row=1;row<44;row++){double h=row*.075;
    walls.Add(Box(x-.343,cy-.343,z+h,.686,.006,.008,"Junta portada"));
    walls.Add(Box(x-.343,cy+.337,z+h,.686,.006,.008,"Junta portada"));
    walls.Add(Box(x-.343,cy-.337,z+h,.006,.674,.008,"Junta portada"));
    walls.Add(Box(x+.337,cy-.337,z+h,.006,.674,.008,"Junta portada"));
    foreach(double s in row%2==0?new[]{-.22,.06}:new[]{-.08,.20}){
     walls.Add(Box(x-.344,cy+s,z+h,.006,.008,.067,"Junta portada"));
     walls.Add(Box(x+.338,cy+s,z+h,.006,.008,.067,"Junta portada"));
    }
   }
   detail.Add(Rod(P(x-1.3,cy,z+3.66),P(x+1.3,cy,z+3.66),.045,"Madera oscura",4));
   detail.Add(Rod(P(x-.3,cy,z+3.28),P(x-1.1,cy,z+3.66),.022,"Carpinteria",4));
   detail.Add(Rod(P(x+.3,cy,z+3.28),P(x+1.1,cy,z+3.66),.022,"Carpinteria",4));
  }
  // Stepped side wings seen at 08:02; lengths and heights remain approximate.
  foreach(double side in new[]{-1d,1d})for(int n=0;n<3;n++){
   double cy=y+side*(3.28+n*.72),h=1.65-n*.25;
   walls.Add(Box(x-.14,cy-.36,z-.14,.28,.72,h+.14,"Ladrillo portada"));
   walls.Add(Box(x-.19,cy-.39,z+h,.38,.78,.065,"Concreto"));
   for(double zz=.075;zz<h;zz+=.075)foreach(double xx in new[]{x-.144,x+.138})walls.Add(Box(xx,cy-.36,z+zz,.006,.72,.008,"Junta portada"));
  }
  for(double yy=y-3.26;yy<y+3.27;yy+=.20){
   foreach(double side in new[]{-1d,1d})detail.Add(Rod(P(x+side*1.46,yy,z+3.805),P(x,yy,z+3.805+1.46*Math.Tan(.30)),.05,"Teja",8));
  }
  detail.Add(Rod(P(x,y-3.3,z+4.29),P(x,y+3.3,z+4.29),.105,"Teja",12));
  Shape("SITIO - Portada pilares y muros",walls);Shape("SITIO - Portada tejas y cerchas",detail);
  Camera("CAPILLA - 11 Portada de acceso",C(-49,-47,4),C(-38,-34,-2),C(-45,-42,-8),C(-31,-27,4));
  var v=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().First(v=>v.Name=="CAPILLA - 11 Portada de acceso");
  var t=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().First(v=>v.Name=="CAPILLA - Presentacion con sombras");v.ApplyViewTemplateParameters(t);
 });}

 static double[] NC={32.94761590338957,51.664271694742816};
 static double[] NT={27.58629496291247,65.6193581945095};
 static double[] NS={-7.118855623385877,36.06138757408541};
 static double[] NK={1.0,36.0};
 static double[] NL={-50.24373418343728,-5.938301791669189};
 static double[] NI={-24.576322425986476,-61.09194449518144};
 static double[][] NP={new[]{9d,26,5,4.5},new[]{-1.844400263977052,21.956044987487793,3.4,5},new[]{2d,-50,5.6,3.8}};
 static double[][] RearRoute={new[]{0d,10.73,1.73},new[]{0d,12,1.77},new[]{-2.6,15.1,1.94},new[]{-7.4,17.7,2.2},new[]{-8.2,22,2.55},new[]{-8d,28,3.35},new[]{-13.3,32.2,3.90},new[]{-14.2,39.45,5.17}};
 static double[][] DriveRoute={new[]{15d,-24},new[]{21d,-15},new[]{26d,-3},new[]{32d,10}};
 static void NMark(Element e,string n){e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.Set("NUEVAS_1210 - "+n+" - posiciones del propietario y videos 12-10-2026; dimensiones estimadas");}
 static void NRemove(string prefix){foreach(var e in new FilteredElementCollector(D).WhereElementIsNotElementType().Where(e=>(e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString()??"").StartsWith("NUEVAS_1210 - "+prefix)).ToList())if(e.IsValidObject&&e.Id.Value!=154763)D.Delete(e.Id);foreach(var e in new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(e=>e.ApplicationDataId.StartsWith("1210 "+prefix)).ToList())if(e.IsValidObject)D.Delete(e.Id);}
 static void NAt(long id,double x,double y,double dz=0){var e=D.GetElement(E(id));if(e==null)return;var b=e.get_BoundingBox(null);var c=(b.Min+b.Max)*.1524;var delta=P(x-c.X,y-c.Y,dz);if(delta.GetLength()>1e-7)ElementTransformUtils.MoveElement(D,e.Id,delta);}
 static void NM(long id,double dx,double dy,double dz=0){if(D.GetElement(E(id))!=null)ElementTransformUtils.MoveElement(D,E(id),P(dx,dy,dz));}
 static double NLvl(int j){return j==2?Base(2,-50)-.4:Base(NP[j][0],NP[j][1])+.25;}
 static double[] NPoint(int j,double a,double scale=1){var q=NP[j];double xx,yy;if(j==2){xx=Math.Sign(Math.Cos(a))*Math.Pow(Math.Abs(Math.Cos(a)),.45);yy=Math.Sign(Math.Sin(a))*Math.Pow(Math.Abs(Math.Sin(a)),.45);}else{double r=j==1?1+.18*Math.Cos(3*a):1;xx=r*Math.Cos(a);yy=r*Math.Sin(a);}return new[]{q[0]+q[2]*xx*scale,q[1]+q[3]*yy*scale};}
 static double NRaw(double x,double y){double z=Base(x,y)+.24*Math.Sin(x/11)*Math.Sin(y/15);var pads=new[]{new[]{0d,-.7,20,13.8,-.17},new[]{48d,18,11.7,9.5,Base(48,18)-.17},new[]{NL[0],NL[1],11d,9,Base(NL[0],NL[1])-.17},new[]{-50d,12,8,7,Base(-50,12)-.17},new[]{NS[0],NS[1],8.0,5.0,Base(NS[0],NS[1])-.17},new[]{NK[0],NK[1],6.1,6.1,Base(NK[0],NK[1])-.17},new[]{NC[0],NC[1],6.6,15d,Base(NC[0],NC[1])-.17},new[]{NT[0],NT[1],7.2,5.6,Base(NT[0],NT[1])-.17},new[]{26d,30,4.8,4.1,Base(26,30)-.17},new[]{NI[0],NI[1],11d,11,Base(NI[0],NI[1])-.17},new[]{-15d,-48,6.5,6.5,Height(-15,-48)-.1},new[]{-14.2,40.3,2d,2d,5.0}};
  foreach(var p in pads){double dx=Math.Max(Math.Abs(x-p[0])-p[2]/2,0),dy=Math.Max(Math.Abs(y-p[1])-p[3]/2,0),d=Math.Sqrt(dx*dx+dy*dy);if(d<4){double t=d/4;t=t*t*(3-2*t);z=p[4]*(1-t)+z*t;}}
  for(int j=0;j<3;j++){var q=NP[j];double xx=(x-q[0])/q[2],yy=(y-q[1])/q[3];double a=Math.Atan2(yy,xx);double r=j==2?Math.Pow(Math.Pow(Math.Abs(xx),4.4)+Math.Pow(Math.Abs(yy),4.4),1/4.4):Math.Sqrt(xx*xx+yy*yy)/(j==1?1+.18*Math.Cos(3*a):1);double level=NLvl(j);if(r<.98)z=level-.9;else if(r<1.40){double t=Math.Max(0,Math.Min(1,(r-.98)/.08));z=(level-.9)*(1-t)+(level-.35)*t;}else if(r<1.9){double t=(r-1.4)/.5;t=t*t*(3-2*t);z=(level-.35)*(1-t)+z*t;}}
  return z;
 }
 static double NHeight(double x,double y){double z=NRaw(x,y),best=1e9,ground=z;for(int i=0;i<RearRoute.Length-1;i++){var a=RearRoute[i];var b=RearRoute[i+1];double dx=b[0]-a[0],dy=b[1]-a[1],t=Math.Max(0,Math.Min(1,((x-a[0])*dx+(y-a[1])*dy)/(dx*dx+dy*dy)));double d=Math.Sqrt(Math.Pow(x-a[0]-t*dx,2)+Math.Pow(y-a[1]-t*dy,2));if(d<best){best=d;ground=a[2]+t*(b[2]-a[2])-.17;}}if(best<2.0){double t=Math.Max(0,(best-.8)/1.2);t=t*t*(3-2*t);z=ground*(1-t)+z*t;}if(y>=6.55&&y<=10.73&&Math.Abs(x)<2){double gz=.08+(y-6.55)/4.18*1.65-.18,t=Math.Max(0,(Math.Abs(x)-.8)/1.2);t=t*t*(3-2*t);z=gz*(1-t)+z*t;}return z;}
 static void NLayout(){Tx("Alinear piezas respetando movimientos del propietario",()=>{
  double dz=Base(NC[0],NC[1])-Base(55,50);NAt(165657,NC[0],NC[1],dz);NAt(165666,NC[0],NC[1],dz);NAt(165712,NC[0]+.01,NC[1],dz);
  dz=Base(NT[0],NT[1])-Base(58,64);NAt(165715,NT[0],NT[1],dz);NM(165720,0,0,dz);
  dz=Base(NI[0],NI[1])-Base(-24,-60);NAt(165827,NI[0],NI[1],dz);NM(165836,0,0,dz);NM(166061,NI[0]+24,NI[1]+60,dz);
  // Roof moved independently by the owner: bring its walls, slab and wall details with it.
  double dx=NL[0]+30,dy=NL[1]-6;dz=Base(NL[0],NL[1])-Base(-30,6);
  foreach(long id in new long[]{154895,154896,154897,154898}){var w=Get<Wall>(id);foreach(var a in w.GetAttachmentIds(AttachmentLocation.Top).ToList())w.RemoveAttachment(a);}
  foreach(long id in new long[]{154895,154896,154897,154898,154747,165264})NM(id,dx,dy,dz);NM(155386,0,0,dz);D.Regenerate();foreach(long id in new long[]{154895,154896,154897,154898})Get<Wall>(id).AddAttachment(E(155386),AttachmentLocation.Top);
  foreach(long id in new long[]{165657,165666,165712,165715,165720,165827,165836,166061})NMark(D.GetElement(E(id)),"Posicion alineada con movimiento del propietario");
 });}
 static void NShelters(){Tx("Caballerizas y cocina triangular observadas",()=>{
  NRemove("Caballerizas");NRemove("Cocina");Del(166045);Del(166060);
  double x=NS[0],y=NS[1],z=Base(x,y);var floor=Get<Floor>(154763);floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set((z+.03)*U);NMark(floor,"Caballerizas - solera en posicion del propietario");
  var roof=SiteRoof("Caballerizas - cubierta baja",x,y,8.0,5.4,z+2.35,.10,false,"Verde gallinero");NMark(roof,"Caballerizas - cubierta baja");var g=new List<GeometryObject>();
  g.Add(Box(x-3.5,y+1.80,z,7,.15,2.15,"Blanco"));
  for(int i=0;i<4;i++){double xx=x-3.5+i*1.75;g.Add(Box(xx,y-1.7,z,.12,3.5,2.12,"Blanco"));if(i>0){g.Add(Box(xx+.12,y-1.75,z,.20,.14,2.1,"Blanco"));g.Add(Box(xx+1.43,y-1.75,z,.32,.14,2.1,"Blanco"));g.Add(Box(xx+.32,y-1.75,z+1.95,1.11,.14,.17,"Blanco"));g.Add(Box(xx+.35,y-1.78,z+.02,.78,.09,1.87,"Verde gallinero"));g.Add(Box(xx+1.15,y-1.78,z+1.10,.28,.09,.68,"Verde gallinero"));}}
  g.Add(Box(x+3.4,y-1.7,z,.12,3.5,2.12,"Blanco"));for(double zz=.25;zz<1.1;zz+=.23)g.Add(Box(x-3.5,y-1.86,z+zz,1.7,.09,.10,"Madera"));foreach(double xx in new[]{x-3.5,x-1.75,x+3.5})g.Add(Rod(P(xx,y-2.1,z),P(xx,y-2.1,z+2.38),.065,"Tronco",8));Shape("1210 Caballerizas - divisiones y puertas verdes",g);
  x=NK[0];y=NK[1];z=Base(x,y);var f=SiteSlab("Cocina - solera",Rectangle(x,y,5.4,5.7),z+.04,"Concreto");NMark(f,"Cocina - solera");
  var r=SiteRoof("Cocina triangular - cubierta verde",x,y,6.1,6.1,z+2.12,.56,false,"Verde gallinero");NMark(r,"Cocina - cubierta triangular");
  var steel=new List<GeometryObject>();for(int iy=0;iy<3;iy++){double yy=y-2.55+iy*2.55;foreach(double s in new[]{-1d,1d}){
   double xx=x+s*2.35;steel.Add(Box(xx-.07,yy-.07,z,.14,.14,.20,"Concreto claro"));steel.Add(Rod(P(xx,yy,z+.1),P(xx,yy,z+2.54),.027,"Verde gallinero",4));steel.Add(Rod(P(xx+s*.14,yy,z+.1),P(xx+s*.14,yy,z+2.54),.023,"Verde gallinero",4));
   for(double h=.5;h<2.4;h+=.55)steel.Add(Rod(P(xx,yy,z+h),P(xx+s*.14,yy,z+h),.017,"Verde gallinero",4));
   XYZ e=P(x+s*3.0,yy,z+2.12),tip=P(x,yy,z+2.12+3.05*.56);steel.Add(Rod(e,tip,.035,"Verde gallinero",4));steel.Add(Rod(e-P(0,0,.2),tip-P(0,0,.2),.025,"Verde gallinero",4));for(int k=0;k<7;k++){var a=e+(tip-e)*(k/7d);var b=e+(tip-e)*((k+1)/7d)-P(0,0,.2);steel.Add(Rod(a,b,.019,"Verde gallinero",4));}
  }}
  for(double xx=x-2.85;xx<x+3;xx+=.76){double zz=z+2.12+(3.05-Math.Abs(xx-x))*.56+.14;steel.Add(Rod(P(xx,y-3.05,zz),P(xx,y+3.05,zz),.009,"Juntas cubierta",4));}
  Shape("1210 Cocina - estructura metalica verde",steel);
  var equip=new List<GeometryObject>();SiteMat("Acero cocina",156,165,163);SiteMat("Hueco fregadero",56,65,64);
  equip.Add(Box(x+1.45,y-.75,z,.6,2.8,.68,"Ladrillo portada"));equip.Add(Box(x+1.38,y-.82,z+.81,.78,2.95,.07,"Acero cocina"));equip.Add(Box(x+1.44,y-.66,z+.876,.6,.62,.006,"Hueco fregadero"));equip.Add(Box(x+1.44,y+.18,z+.876,.6,.62,.006,"Hueco fregadero"));equip.Add(Box(x+2.09,y-.82,z+.81,.06,2.95,.35,"Acero cocina"));
  foreach(double yy in new[]{y-.3,y+.55}){equip.Add(Rod(P(x+1.9,yy,z+.88),P(x+1.9,yy,z+1.20),.018,"Acero cocina",8));equip.Add(Rod(P(x+1.9,yy,z+1.20),P(x+1.70,yy,z+1.20),.018,"Acero cocina",8));}
  equip.Add(Box(x-.58,y+1.7,z+.30,1.16,.64,1.65,"Acero cocina"));for(int i=0;i<5;i++)equip.Add(Box(x-.50,y+1.65,z+.46+i*.29,1,.035,.035,"Carpinteria"));equip.Add(Box(x-.48,y+1.66,z+.45,.96,.025,1.35,"Vidrio"));
  equip.Add(Box(x-2.2,y+.55,z+.80,1.65,.65,.07,"Blanco"));foreach(double xx in new[]{x-2.1,x-.65})foreach(double yy in new[]{y+.65,y+1.1})equip.Add(Rod(P(xx,yy,z),P(xx,yy,z+.8),.025,"Acero cocina",6));equip.Add(Rod(P(x-.4,y-.45,z),P(x-.4,y-.45,z+.79),.24,"Tronco",24));equip.Add(Box(x-.92,y-.97,z+.79,1.04,1.04,.075,"Concreto"));
  Shape("1210 Cocina - fregadero horno y mesas",equip,BuiltInCategory.OST_Furniture);
 });}
 static void NLakes(){Tx("Lagos contiguos con bordes de ladrillo",()=>{
  foreach(long id in new long[]{165901,165902,165903,165913,165914,165915})Del(id);NRemove("Lago");
  SiteMat("Ladrillo lago",165,144,108);SiteMat("Junta lago",203,190,158);
  for(int j=0;j<3;j++){double z=NLvl(j);var ring=new Mesh();var joints=new Mesh();int count=160;
   for(int i=0;i<count;i++){double a=i*2*Math.PI/count,b=(i+1)*2*Math.PI/count;double[] p=NPoint(j,a),q=NPoint(j,b),o=NPoint(j,a,1.055),v=NPoint(j,b,1.055);
    ring.Quad(P(p[0],p[1],z+.13),P(q[0],q[1],z+.13),P(v[0],v[1],z+.13),P(o[0],o[1],z+.13),"Borde lago");
    ring.Quad(P(o[0],o[1],z-.85),P(v[0],v[1],z-.85),P(v[0],v[1],z+.13),P(o[0],o[1],z+.13),j==2?"Piedra oscura":"Ladrillo lago");
    if(j<2)for(int row=0;row<10;row++){double h=z+.08-row*.09;double[] jo=NPoint(j,a,1.056),jv=NPoint(j,b,1.056);joints.Quad(P(jo[0],jo[1],h),P(jv[0],jv[1],h),P(jv[0],jv[1],h+.006),P(jo[0],jo[1],h+.006),"Junta lago");if((i+row)%3==0){double c=a+.0016;var jq=NPoint(j,c,1.056);joints.Quad(P(jo[0],jo[1],h),P(jq[0],jq[1],h),P(jq[0],jq[1],h+.084),P(jo[0],jo[1],h+.084),"Junta lago");}}
   }
   var gg=new List<GeometryObject>();gg.AddRange(ring.Build());gg.AddRange(joints.Build());Shape("1210 Lago "+(j+1)+" - vaso y borde",gg);
   var pts=Enumerable.Range(0,count).Select(i=>{var p=NPoint(j,i*2*Math.PI/count);return P(p[0],p[1],z-.015);}).ToList();var solid=GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>{Loop(pts)},XYZ.BasisZ,.03*U,new SolidOptions(M("Agua sitio opaca"),ElementId.InvalidElementId));Shape("1210 Lago "+(j+1)+" - agua",new List<GeometryObject>{solid});
   if(j<2){var q=NP[j];var pipes=new List<GeometryObject>{Rod(P(q[0],q[1],z-.15),P(q[0],q[1],z+.73),.025,"Blanco",10),Rod(P(q[0],q[1],z+.73),P(q[0]+.65,q[1],z+.73),.024,"Blanco",10)};Shape("1210 Lago "+(j+1)+" - tuberia visible",pipes);}
  }
 });}
 static void NTerrain(){Tx("Terreno adaptado a ubicaciones corregidas",()=>{
  var old=new FilteredElementCollector(D).OfClass(typeof(Toposolid)).Cast<Toposolid>().Single();var type=old.GetTypeId();D.Delete(old.Id);
  var pts=new Dictionary<string,XYZ>();Action<double,double> add=(x,y)=>pts[x.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+","+y.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)]=P(x,y,NHeight(x,y));
  for(double x=-110;x<=110;x+=5)for(double y=-95;y<=105;y+=5)add(x,y);
  for(double x=-19;x<=21;x+=1)for(double y=5;y<=44;y+=1)add(x,y);
  foreach(var p in new[]{new[]{NC[0],NC[1],8d,17},new[]{NT[0],NT[1],9d,8},new[]{NS[0],NS[1],10d,7},new[]{NK[0],NK[1],8d,8},new[]{NL[0],NL[1],13d,11},new[]{NI[0],NI[1],14d,14}})for(double x=p[0]-p[2]/2;x<=p[0]+p[2]/2;x+=1)for(double y=p[1]-p[3]/2;y<=p[1]+p[3]/2;y+=1)add(x,y);
  foreach(var q in NP)for(double x=q[0]-q[2]*2;x<=q[0]+q[2]*2;x+=.65)for(double y=q[1]-q[3]*2;y<=q[1]+q[3]*2;y+=.65)add(x,y);
  for(int j=0;j<3;j++)for(int i=0;i<128;i++)foreach(double r in new[]{.97,1.02,1.40}){var p=NPoint(j,i*Math.PI/64,r);add(p[0],p[1]);}
  for(double x=-11;x<=11;x+=1)for(double y=-9;y<=7;y+=1)add(x,y);
  var topo=Toposolid.Create(D,pts.Values.ToList(),type,L.Id);NMark(topo,"Relieve para posiciones del propietario y recorrido posterior");
  RemoveShape("Borduras vegetales del recorrido principal");
  var removed=new List<long>();foreach(var plant in new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(e=>e.Category.Id.Value==(long)BuiltInCategory.OST_Planting).ToList()){
   var b=plant.get_BoundingBox(null);if(b==null)continue;var c=(b.Min+b.Max)*.1524;double x=c.X,y=c.Y;
   bool hit=NP.Any(q=>Math.Pow((x-q[0])/(q[2]+.55),2)+Math.Pow((y-q[1])/(q[3]+.55),2)<1)||new[]{new[]{NC[0],NC[1],4d,8.0},new[]{NT[0],NT[1],4d,3.3},new[]{NS[0],NS[1],4.6,3.2},new[]{NK[0],NK[1],3.5,3.5}}.Any(q=>Math.Abs(x-q[0])<q[2]&&Math.Abs(y-q[1])<q[3]);
   if(hit){removed.Add(plant.Id.Value);D.Delete(plant.Id);continue;}
   if(b.Max.X-b.Min.X<45*U&&plant.ApplicationDataId!="Borduras vegetales del recorrido principal"){double dz=NHeight(x,y)-b.Min.Z*.3048;if(Math.Abs(dz)>.07&&Math.Abs(dz)<5)ElementTransformUtils.MoveElement(D,plant.Id,P(0,0,dz));}
  }Notes.Add("Plantas en conflicto recolocadas mediante limpieza de "+removed.Count+" ejemplares esquematicos");
 });}
 static List<double[]> NRouteSample(double[][] line,double spacing){var p=new List<double[]>();for(int i=0;i<line.Length-1;i++){var a=line[i];var b=line[i+1];int count=(int)Math.Ceiling(Math.Sqrt(Math.Pow(b[0]-a[0],2)+Math.Pow(b[1]-a[1],2))/spacing);for(int j=0;j<count;j++){double t=(double)j/count;p.Add(new[]{a[0]+t*(b[0]-a[0]),a[1]+t*(b[1]-a[1]),a[2]+t*(b[2]-a[2])});}}p.Add(line.Last());return p;}
 static Floor NWalkFloor(string n,IList<XYZ> points){var type=new FilteredElementCollector(D).OfClass(typeof(FloorType)).Cast<FloorType>().FirstOrDefault(f=>f.Name=="Capilla nueva - cemento sendero");if(type==null){type=(FloorType)Get<FloorType>(1238).Duplicate("Capilla nueva - cemento sendero");var cs=type.GetCompoundStructure();for(int i=0;i<cs.LayerCount;i++){cs.SetMaterialId(i,M("Concreto"));if(i==0)cs.SetLayerWidth(i,.10*U);}type.SetCompoundStructure(cs);}var f=Floor.Create(D,new List<CurveLoop>{Loop(points.Select(p=>new XYZ(p.X,p.Y,0)))},type.Id,L.Id);D.Regenerate();var ed=f.GetSlabShapeEditor();ed.Enable();D.Regenerate();foreach(SlabShapeVertex v in ed.SlabShapeVertices){var p=points.OrderBy(p=>Math.Pow(p.X-v.Position.X,2)+Math.Pow(p.Y-v.Position.Y,2)).First();ed.ModifySubElement(v,p.Z);}NMark(f,n);return f;}
 static void NTuft(Mesh mesh,double x,double y,double ground){for(int k=0;k<12;k++){double a=k*Math.PI/6+.17*Math.Sin(x+y),dx=Math.Cos(a),dy=Math.Sin(a);var last=P(x,y,ground);for(int j=1;j<=3;j++){double t=j/3d,r=.34*t*t;var cur=P(x+dx*r,y+dy*r,ground+.47*Math.Sin(t*1.8));var side=P(-dy*.012,dx*.012,0);mesh.Quad(last-side,last+side,cur+side,cur-side,k%3==0?"Blanco":"Borde vegetal");last=cur;}}}
 static void NPaths(){Tx("Salida posterior escalones y sendero junto a lagos",()=>{
  NRemove("Recorrido");NRemove("Porche");Del(165918);RemoveShape("Bordillos y juntas de pavimentos");
  // Add the observed rear veranda and a real hosted door without reconstructing unseen rooms.
  foreach(var f in new FilteredElementCollector(D).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().Where(f=>(f.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString()??"").StartsWith("NUEVAS_1210 - Salida trasera")).ToList())D.Delete(f.Id);
  var door=D.Create.NewFamilyInstance(P(0,4.5,0),Get<FamilySymbol>(67177),Get<Wall>(154885),L,Autodesk.Revit.DB.Structure.StructuralType.NonStructural);NMark(door,"Salida trasera hacia lagos");
  var veranda=SiteSlab("Porche posterior",Rectangle(0,5.525,18,2.05),.055,"Terracota baldosa");NMark(veranda,"Porche posterior - piso");
  foreach(long id in new long[]{154879,154882,154885,154888})foreach(var a in Get<Wall>(id).GetAttachmentIds(AttachmentLocation.Top).ToList())Get<Wall>(id).RemoveAttachment(a);Del(163468);
  foreach(var r in new FilteredElementCollector(D).OfClass(typeof(RoofBase)).Where(r=>(r.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString()??"").StartsWith("NUEVAS_1210 - Porche - cubierta casa")).ToList())D.Delete(r.Id);
  var roof=SiteRoof("Casa principal con porche posterior",0,-.475,20,15.15,3.08,.23,true,"Techo verde");NMark(roof,"Porche - cubierta casa prolongada");foreach(long id in new long[]{154879,154882,154885,154888})Get<Wall>(id).AddAttachment(roof.Id,AttachmentLocation.Top);
  RemoveShape("Aleros canalones y bajantes");RemoveShape("Juntas de laminas limatesas y cumbrera");var detail=new List<GeometryObject>();
  foreach(double y in new[]{-8.05,7.1})detail.Add(Box(-10,y,3.05,20,.12,.16,"Baranda"));foreach(double x in new[]{-10d,9.94})detail.Add(Box(x,-8.05,3.05,.12,15.15,.16,"Baranda"));
  Func<double,double,double> rz=(x,y)=>3.08+.23*Math.Min(Math.Min(x+10,10-x),Math.Min(y+8.05,7.1-y))+.145;
  for(double x=-9.75;x<10;x+=.76)for(double y=-7.96;y<7.08;y+=.5){double yn=Math.Min(y+.5,7.08);detail.Add(Rod(P(x,y,rz(x,y)),P(x,yn,rz(x,yn)),.012,"Juntas cubierta",4));}
  foreach(double x in new[]{-9d,-6,-3,-.8,.8,3,6,9})detail.Add(Box(x-.065,6.43,.055,.13,.13,2.97,"Madera oscura"));detail.Add(Box(-9.1,6.42,2.89,18.2,.18,.2,"Madera oscura"));
  foreach(double side in new[]{-1d,1d}){double a=side<0?-9:.8,b=side<0?-.8:9;detail.Add(Box(a,6.44,.94,b-a,.1,.085,"Madera oscura"));detail.Add(Box(a,6.44,.22,b-a,.08,.07,"Madera oscura"));for(double x=a+.1;x<b;x+=.18)detail.Add(Box(x,6.45,.28,.04,.045,.65,"Madera oscura"));}
  for(double x=-8.99;x<9;x+=.48)for(double y=4.51;y<6.54;y+=.24)detail.Add(Box(x,y,.056,Math.Min(.473,9-x),Math.Min(.233,6.54-y),.012,"Terracota baldosa"));
  Shape("1210 Porche - estructura baranda baldosas y cubierta",detail);
  var ceiling=new Mesh();for(double y=4.55;y<6.9;y+=.14)ceiling.Quad(P(-9.3,y,2.97),P(9.3,y,2.97),P(9.3,y+.132,2.97),P(-9.3,y+.132,2.97),"Madera cielo");Shape("1210 Porche - cielo de madera",ceiling.Build());
  var steps=new List<GeometryObject>();for(int i=0;i<11;i++){double y=6.55+i*.38,top=.08+(i+1)*.15,bottom=Math.Min(NHeight(-.65,y),NHeight(.65,y))-.13;steps.Add(Box(-.65,y,bottom,1.30,.384,top-bottom,"Concreto"));}Shape("1210 Recorrido - once escalones posteriores aproximados",steps);
  var route=NRouteSample(RearRoute,1.15);var xy=route.Select(p=>new[]{p[0],p[1]}).ToList();var left=Offset(xy,.6);var right=Offset(xy,-.6);var edges=new List<GeometryObject>();var grass=new Mesh();
  for(int i=0;i<route.Count-1;i++){
   var a=route[i];var b=route[i+1];NWalkFloor("Recorrido posterior - tramo "+i,new[]{P(left[i][0],left[i][1],a[2]),P(right[i][0],right[i][1],a[2]),P(right[i+1][0],right[i+1][1],b[2]),P(left[i+1][0],left[i+1][1],b[2])});
   foreach(var side in new[]{left,right})edges.Add(Rod(P(side[i][0],side[i][1],a[2]-.015),P(side[i+1][0],side[i+1][1],b[2]-.015),.035,"Concreto claro",4));
  }
  foreach(double side in new[]{-1d,1d})foreach(var p in Offset(xy,side*.9))NTuft(grass,p[0],p[1],NHeight(p[0],p[1]));
  foreach(double side in new[]{-1d,1d})for(double y=6.7;y<10.8;y+=.40)NTuft(grass,side*.98,y,NHeight(side*.98,y));
  Shape("1210 Recorrido - bordes de cemento",edges);Shape("1210 Recorrido - borduras claras observadas",grass.Build(),BuiltInCategory.OST_Planting);
  var bench=new List<GeometryObject>();double bx=-10.4,by=29.1,bz=NHeight(bx,by);bench.Add(Box(bx-.7,by,bz+.43,1.4,.43,.055,"Blanco"));for(double xx=bx-.65;xx<=bx+.7;xx+=.32){bench.Add(Rod(P(xx,by+.39,bz+.48),P(xx,by+.39,bz+.99),.014,"Blanco",8));}foreach(double xx in new[]{bx-.62,bx+.62})foreach(double yy in new[]{by+.04,by+.38})bench.Add(Rod(P(xx,yy,bz),P(xx,yy,bz+.45),.018,"Blanco",8));Shape("1210 Recorrido - banco junto al lago",bench,BuiltInCategory.OST_Furniture);
 });}
 static void NViews(){Tx("Vistas nuevas tomas y correcciones de ubicacion",()=>{
  Camera("CAPILLA - 12 Recorrido trasero y dos lagos",C(42,-30,48),C(1,21,2.5),C(-18,-10,-3),C(39,46,24));
  Camera("CAPILLA - 13 Caballerizas y cocina",C(14,15,17),C(-3,36,6),C(-16,29,1),C(6,43,15));
  Camera("CAPILLA - 14 Gallinero y tanques reubicados",C(70,15,45),C(33,45,7),C(19,9,-1),C(58,72,25));
  Camera("CAPILLA - 15 Salida trasera hacia lagos",C(-17,28,14),C(0,7,1.3),C(-13,-9,-1),C(18,33,16));
  Camera("CAPILLA - 16 Conjunto nuevas tomas",C(54,-128,93),C(4,3,2),C(-66,-73,-15),C(67,77,33));
  var template=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().First(v=>v.Name=="CAPILLA - Presentacion con sombras");
  foreach(var v in new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().Where(v=>!v.IsTemplate&&Enumerable.Range(12,5).Any(i=>v.Name.StartsWith("CAPILLA - "+i)))){
   v.ApplyViewTemplateParameters(template);var box=v.GetSectionBox();var hide=new List<ElementId>();foreach(var e in new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(e=>e.Category.Id.Value==(long)BuiltInCategory.OST_Planting)){var b=e.get_BoundingBox(null);var c=(b.Min+b.Max)/2;if(c.X<box.Min.X||c.X>box.Max.X||c.Y<box.Min.Y||c.Y>box.Max.Y||b.Max.Z>box.Max.Z)if(e.CanBeHidden(v))hide.Add(e.Id);}if(hide.Count>0)v.HideElements(hide);
   foreach(Category cat in D.Settings.Categories)foreach(Category sub in cat.SubCategories)if(sub.Name.IndexOf("curva",StringComparison.OrdinalIgnoreCase)>=0||sub.Name.IndexOf("contour",StringComparison.OrdinalIgnoreCase)>=0)try{v.SetCategoryHidden(sub.Id,true);}catch{}
  }
 });}
 static void NExtras(){Tx("Huellas de cemento y caseta al final del recorrido",()=>{
  Del(164479);NRemove("Huellas");NRemove("Servicio");
  var line=Sample(DriveRoute,1.5);var joints=new List<GeometryObject>();
  foreach(double s in new[]{-1d,1d}){var center=Offset(line,s*.70);var a=Offset(center,.45);var b=Offset(center,-.45);for(int i=0;i<line.Count-1;i++)NWalkFloor("Huellas de cemento - "+s+" - "+i,new[]{P(a[i][0],a[i][1],NHeight(a[i][0],a[i][1])+.14),P(b[i][0],b[i][1],NHeight(b[i][0],b[i][1])+.14),P(b[i+1][0],b[i+1][1],NHeight(b[i+1][0],b[i+1][1])+.14),P(a[i+1][0],a[i+1][1],NHeight(a[i+1][0],a[i+1][1])+.14)});}
  foreach(double s in new[]{-1d,1d}){var p=Offset(line,s*1.40);for(int i=0;i<p.Count-1;i++)joints.Add(Rod(P(p[i][0],p[i][1],NHeight(p[i][0],p[i][1])+.07),P(p[i+1][0],p[i+1][1],NHeight(p[i+1][0],p[i+1][1])+.07),.045,"Concreto",4));}Shape("1210 Huellas - cunetas laterales",joints);
  double x=-14.2,y=40.3,z=5.17;var slab=SiteSlab("Servicio - piso",Rectangle(x,y,1.9,1.9),z+.03,"Concreto");NMark(slab,"Servicio - piso de caseta observada");var roof=SiteRoof("Caseta de servicio",x,y,2.1,2.0,z+2.25,.06,false,"Metal gris");NMark(roof,"Servicio - cubierta");
  var g=new List<GeometryObject>{Box(x-.75,y-.7,z,.12,1.5,2.15,"Blanco"),Box(x+.63,y-.7,z,.12,1.5,2.15,"Blanco"),Box(x-.75,y+.68,z,1.5,.12,2.15,"Blanco"),Box(x-.75,y-.7,z,.35,.12,2.15,"Blanco"),Box(x+.37,y-.7,z,.38,.12,2.15,"Blanco"),Box(x-.4,y-.7,z+2.0,.77,.12,.15,"Blanco"),Box(x-.38,y-.73,z+.02,.73,.08,1.98,"Madera cielo")};Shape("1210 Servicio - caseta blanca puerta de madera",g);
 });}
 static void NGroups(){Tx("Agrupar componentes para futuros movimientos",()=>{
  Action<string,IEnumerable<ElementId>> group=(n,ids)=>{var list=ids.Where(id=>D.GetElement(id)!=null&&D.GetElement(id).GroupId==ElementId.InvalidElementId).Distinct().ToList();if(list.Count>1){var g=D.Create.NewGroup(list);g.GroupType.Name="CAPILLA - "+n;NMark(g,"Conjunto movible "+n);}};
  group("Gallinero",new[]{E(165657),E(165666),E(165712)});group("Tanques",new[]{E(165715),E(165720)});group("Kiosco inferior",new[]{E(165827),E(165836),E(166061)});
  for(int j=1;j<=3;j++)group("Lago "+j,new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(e=>e.ApplicationDataId.StartsWith("1210 Lago "+j+" - ")).Select(e=>e.Id));
  foreach(string n in new[]{"Caballerizas","Cocina"})group(n,new FilteredElementCollector(D).WhereElementIsNotElementType().Where(e=>e is Floor||e is RoofBase||e is DirectShape).Where(e=>(e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString()??"").StartsWith("NUEVAS_1210 - "+n)||(e as DirectShape)?.ApplicationDataId.StartsWith("1210 "+n+" - ")==true).Select(e=>e.Id));
 });}

 public static object Execute(Document doc,UIApplication ui,string action){D=doc;UI=ui;if(!D.PathName.EndsWith("Capilla_ajuste_nuevas_tomas.rvt",StringComparison.OrdinalIgnoreCase))throw new Exception("Abrir primero la copia Capilla_ajuste_nuevas_tomas.rvt");LoadMats();
  if(action=="nextras")NExtras();else if(action=="nlayout")NLayout();else if(action=="nshelters")NShelters();else if(action=="nterrain")NTerrain();else if(action=="nlakes")NLakes();else if(action=="npaths")NPaths();else if(action=="nviews")NViews();else if(action=="ngroups")NGroups();else if(action=="siteportal")SitePortal();else if(action=="siteshed")SiteShed();else if(action=="sitepolish")SitePolish();else if(action=="sitebuildings")SiteBuildings();else if(action=="sitelakes")SiteLakes();else if(action=="siteviews")SiteViews();else if(action=="walkingfix")WalkingFix();else if(action=="walking")Walking();else if(action=="walkingviews")WalkingViews();else if(action=="materials")Materials();else if(action=="terrain")Terrain();else if(action=="architecture")Architecture();else if(action=="landscape")Landscape();else if(action.StartsWith("plants-"))Planting(int.Parse(action.Substring(7)));else if(action=="views")Views();else if(action=="finishes")Finishes();else if(action=="details")Details();else if(action=="polish")Polish();else if(action=="coordination")Coordination();else if(action=="texturefix")Tx("escala de texturas",()=>{Texture("Cesped","grass_high_quality_color.jpg",3);Texture("Concreto","concrete_cast-in-place_color.jpg",2);Texture("Estuco","Finishes.Plaster.Stucco.Troweled.Light Yellow.jpg",2);});else if(action.StartsWith("export-"))Export(action.Substring(7));else if(action=="check")return Check();else throw new Exception("Unknown stage "+action);
  D.Save();return new {stage=action,path=D.PathName,notes=Notes};
 }
}
}


