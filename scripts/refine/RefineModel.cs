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
 static void Export(string which){var views=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().Where(v=>!v.IsTemplate&&v.Name.StartsWith("CAPILLA - ")&&(which=="all"||v.Name.Contains(which))).ToList();foreach(var v in views){var options=new ImageExportOptions{FilePath=Path.Combine(Root,"renders","detalle"),ExportRange=ExportRange.SetOfViews,HLRandWFViewsFileType=ImageFileType.PNG,ShadowViewsFileType=ImageFileType.PNG,ImageResolution=ImageResolution.DPI_300,ZoomType=ZoomFitType.FitToPage,PixelSize=2400};options.SetViewsAndSheets(new List<ElementId>{v.Id});D.ExportImage(options);}Notes.Add("Exportadas "+views.Count+" vistas");}
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
 public static object Execute(Document doc,UIApplication ui,string action){D=doc;UI=ui;if(!D.PathName.EndsWith("Capilla_detallada.rvt",StringComparison.OrdinalIgnoreCase))throw new Exception("Abrir primero la copia Capilla_detallada.rvt");LoadMats();
  if(action=="materials")Materials();else if(action=="terrain")Terrain();else if(action=="architecture")Architecture();else if(action=="landscape")Landscape();else if(action.StartsWith("plants-"))Planting(int.Parse(action.Substring(7)));else if(action=="views")Views();else if(action=="finishes")Finishes();else if(action=="details")Details();else if(action=="polish")Polish();else if(action=="coordination")Coordination();else if(action=="texturefix")Tx("escala de texturas",()=>{Texture("Cesped","grass_high_quality_color.jpg",3);Texture("Concreto","concrete_cast-in-place_color.jpg",2);Texture("Estuco","Finishes.Plaster.Stucco.Troweled.Light Yellow.jpg",2);});else if(action.StartsWith("export-"))Export(action.Substring(7));else if(action=="check")return Check();else throw new Exception("Unknown stage "+action);
  D.Save();return new {stage=action,path=D.PathName,notes=Notes};
 }
}
}
