using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public static class Util {
	
	public static void Init() {}
	
	public static Vector3 aimOffset=Vector3.up*1f;
	public static int fireLayer=1,impactLayer=2;
	public static LayerMask ignoreLayer=~(1<<Physics.kIgnoreRaycastLayer);
	
	
	public static Vector2 Planar(this Vector3 p) {
		return new Vector2(p.x,p.z);
	}
	
	public static void StopAnimWalk(Animator anim) {
		float sp=anim.GetFloat ("Speed");
		sp=Mathf.Lerp (sp,0,1*Time.deltaTime/Mathf.Abs (sp));
		anim.SetFloat ("VSpeed",sp);
		anim.SetFloat ("Speed",sp);
	}
	
	public static IEnumerator WaitUntil(Animator anim,int st,int layer,float dt) {
		float t=Time.time;
		while(Time.time<t+dt) {
			AnimatorStateInfo s=anim.GetCurrentAnimatorStateInfo (layer);
			if(s.nameHash==st) yield break;
			yield return null;
		}
	}
	public static IEnumerator WaitUntil(Animator anim,int st,int layer) {
		yield return WaitUntil (anim,st,layer,5);
	}
	public static IEnumerator WaitUntilTrans(Animator anim,int st,int layer,float dt) {
		float t=Time.time;
		while(Time.time<t+dt) {
			AnimatorStateInfo s=anim.GetCurrentAnimatorStateInfo (layer);
			if(s.nameHash==st&&anim.IsInTransition (layer)) yield break;
			yield return null;
		}
	}
	
	
	public static Transform getAT(GameObject g) {
		GameObject at=UtilEditor.createOrGetGO("AimTargets",null);
		return UtilEditor.createOrGetGO(g.name+"AT",at.transform).transform;
	}
	
	public static bool Turn(Transform t,Vector3 dir,float wsp,float err) {
		dir.y=0;
		if(dir.magnitude<0.01f) return false;
		dir.Normalize ();
		Vector3 f=t.forward;f.y=0;f.Normalize ();
		float ang=Vector3.Angle (f,dir);
		if(ang>wsp*Time.deltaTime) {
			float a=Mathf.Min (ang,wsp*Time.deltaTime);
			Vector3 up=Vector3.Cross (f,dir);if(up.magnitude<0.1f) up=Vector3.up;
			t.Rotate (up,a);
		}
		return (ang<err);
	}
	public static bool TurnSafe(Transform t,Vector3 dir,float wsp,float err) {
		dir.y=0;
		if(dir.magnitude<0.01f) return false;
		dir.Normalize ();
		Vector3 f=t.forward;f.y=0;f.Normalize ();
		float ang=Vector3.Angle (f,dir);
		if(ang>err) {
			float a=Mathf.Min (ang,wsp*Time.deltaTime);
			Vector3 up=Vector3.Cross (f,dir);if(up.magnitude<0.1f) up=Vector3.up;
			t.Rotate (up,a);
		}
		return (ang<err);
	}
	
	public static IEnumerator FullTurn(Transform t,Vector3 dir,float wsp) {
		dir.y=0;
		if(dir.magnitude<0.01f) yield break;
		dir.Normalize ();
		Vector3 f=t.forward;f.y=0;f.Normalize ();
		float ang=Vector3.Angle (f,dir);
		while(ang>20) {
			yield return null;
			float a=Mathf.Min (ang,wsp*Time.deltaTime);
			Vector3 up=Vector3.Cross (f,dir);if(up.magnitude<0.1f) up=Vector3.up;
			t.Rotate (up,a);
			f=t.forward;f.y=0;f.Normalize ();
			ang=Vector3.Angle (f,dir);
		}
	}
	
	public static bool InView(Transform t,Vector3 p,float err) {
		Vector3 dir=p-t.position;
		dir.y=0;dir.Normalize ();
		Vector3 f=t.forward;f.y=0;f.Normalize ();
		float ang=Vector3.Angle (f,dir);
		return (ang<err);
	}
	
	public static bool LOS(Transform a,Transform b) {
		Vector3 du=Vector3.up*1.8f;
		Vector3 p0=a.position+du,p1=b.position+du;
		RaycastHit hit;
		LayerMask l=~(1<<Physics.kIgnoreRaycastLayer);
		while(Physics.Raycast (p0,p1-p0,out hit,Mathf.Infinity,l)) {
			if(hit.transform.IsChildOf (a)||hit.collider.isTrigger) {
				p0=hit.point+(p1-p0).normalized*0.1f;
			} else if(hit.transform.IsChildOf (b)) return true;
			else return false;
		}
		return true;
	}
	
	public static bool IsVisible(Transform a,Transform b,float aov) {
		return Util.InView(a,b.position,aov)&&Util.LOS (a,b);
	}
	
	public static bool NonTriggerHit(Vector3 p,Transform t,Vector3 dir,out RaycastHit hit,LayerMask hitLayer) {
		while(Physics.Raycast (p,dir,out hit,Mathf.Infinity,hitLayer)) {
			if((t!=null&&hit.transform.IsChildOf (t))||hit.collider.isTrigger) {
				p=hit.point+dir.normalized*0.1f;
			}
			else return true;
		}
		return false;
	}
	
	public static bool GUIHit(out Vector3 p) {
		return GUIHit (out p,ignoreLayer,Event.current.mousePosition);
	}
	public static bool GUIHit(out Vector3 p,Vector2 mp) {
		return GUIHit (out p,ignoreLayer,mp);
	}
	public static bool GUIHit(out Vector3 p,LayerMask layer) {
		return GUIHit (out p,layer,Event.current.mousePosition);
	}
	public static bool GUIHit(out Vector3 p,LayerMask layer,Vector2 mp) {
		RaycastHit hit;p=Vector3.zero;
		if(GUIHit (out hit,layer,mp)) {
			p=hit.point;return true;
		}
		return false;
	}
	public static bool GUIHit(out RaycastHit hit) {
		return GUIHit (out hit,ignoreLayer,Event.current.mousePosition);
	}
	public static bool GUIHit(out RaycastHit hit,Vector2 mp) {
		return GUIHit (out hit,ignoreLayer,mp);
	}
	public static bool GUIHit(out RaycastHit hit,LayerMask layer,Vector2 mp) {
		Ray r=HandleUtility.GUIPointToWorldRay (mp);
		while(Physics.Raycast(r,out hit,Mathf.Infinity,layer)) {
			if(hit.collider.isTrigger) {
				r.origin+=r.direction.normalized*(hit.distance+0.1f);
			}
			return true;
		}
		return false;
	}
	public static bool GUIHit(out Vector3 p,Vector3 planePos) {
		Ray r=HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);p=Vector3.zero;
		Plane pl=new Plane(Vector3.up,planePos);float d;
		if(pl.Raycast(r,out d)) {
			p=r.origin+r.direction.normalized*d;
			return true;
		}
		return false;
	}
	
	public class ProgressBar {
		public int pTot,pI,pCT;
		public float pD;
		private int pCoun;
		public void Init() {pCoun=0;pI=0;pD=0f;ind.Clear ();}
		public void UpdateProgress(string s) {
			if(++pCoun<pCT) return;
			pCoun=0;
			EditorUtility.DisplayProgressBar(s+"...", "", Mathf.InverseLerp(0,pTot,++pI));
			pD=(float)pI/(float)pTot;
		}
		public void UpdateProgress(string s,float d) {
			pI=(int)(d*pTot);
			pD=(float)pI/(float)pTot;
			EditorUtility.DisplayProgressBar(s+"...", "", Mathf.InverseLerp(0,pTot,pI));
		}
		public List<float> ind=new List<float>();
		public void Into(float d) {
			if(ind.Count>0) d*=ind[ind.Count-1];
			ind.Add (d);
		}
		public void Out() {ind.RemoveAt (ind.Count-1);}
		public void AddProgress(string s,float d) {
			if(++pCoun<pCT) return;
			pCoun=0;
			pD+=d*ind[ind.Count-1]*(float)pCT;
			//EditorUtility.DisplayProgressBar(s+"...", "", pD);
		}
	}
	
	private static bool isUpper(char c) {return c>='A'&&c<='Z';}
	private static bool isLower(char c) {return c>='a'&&c<='z';}
	public static string FriendlyString(string st) {
		char[] s=new char[st.Length];
		st.CopyTo (0,s,0,st.Length);
		if(isLower (s[0])) s[0]=(char)((int)s[0]+'A'-'a');
		string s1=""+s[0];
		for(int i=1;i<s.Length;i++) {
			if(isLower(s[i])) s1+=s[i];
			else if(isUpper(s[i])) {
				if(i==s.Length-1||!isUpper(s[i+1])) s1+=" "+s[i];
				else s1+=s[i];
			}
			else s1+=" "+s[i];
		}
		return s1;
	}
	public static List<string> typeStimulus=new List<string>();private static int itStimulus;
	public static List<string> typeResponse=new List<string>();private static int itResponse;
	public static System.Type editorClass;
	public class ComponentEdit {
		public bool show=true,edit=false;
		public Component com;
	}
	public class FI {
		public FieldInfo f;public int pr=0;
		public static int Compare(FI a,FI b) {
			if(a.pr==b.pr) return 0;
			if(a.pr<b.pr) return 1;
			return -1;
		}
	}
	public static void MakeDefaultEditor(object o) {
		System.Type t=o.GetType ();
		PropertyInfo[] api=t.GetProperties ();
		foreach(PropertyInfo p in api) {
			if(p.Name.ToLower ().Equals ("name")) continue;
			object ob=p.GetValue (o,null),ob1;
			object[] att=p.GetCustomAttributes (typeof(HideInInspector),true);
			if(att.Length>0) ob1=ob;
			else ob1=MakeSimpleDefaultEditor (ob,p.PropertyType,FriendlyString(p.Name),o);
			if(ob1!=null&&!ob1.Equals (ob)) {
				if(t.IsSubclassOf (typeof(Object)))	Undo.RegisterUndo ((Object)o,p.Name);
				p.SetValue (o,ob1,null);
			}
		}
		
		FieldInfo[] afi=t.GetFields ();
		List<FI> fi=new List<FI>();
		for(int i=0;i<afi.Length;i++) {
			fi.Add (new FI());fi[i].f=afi[i];
			if(t.BaseType.GetField (afi[i].Name)!=null) fi[i].pr=10;
		}
		fi.Sort (FI.Compare);
		foreach(FI fii in fi) {
			FieldInfo f=fii.f;object ob=f.GetValue (o),ob1;
			object[] att=f.GetCustomAttributes (typeof(HideInInspector),true);
			if(att.Length>0) ob1=ob;
			else ob1=MakeSimpleDefaultEditor (ob,f.FieldType,f.Name,o);
			if(ob1!=null&&!ob1.Equals (ob)) {
				if(t.IsSubclassOf (typeof(Object)))	Undo.RegisterUndo ((Object)o,f.Name);
				f.SetValue (o,ob1);
			}
		}
	}
	public static string SOInstance(System.Type t) {
		ScriptableObject so=ScriptableObject.CreateInstance (t);
		string path="Assets/Resources/AIData/"+Selection.activeGameObject.name;
		if(!System.IO.Directory.Exists (path)) AssetDatabase.CreateFolder ("Assets/Resources/AIData",Selection.activeGameObject.name);
		string ext="asset";//(t.IsSubclassOf (typeof(Stimulus))?"st":(t.IsSubclassOf (typeof(Response))?"re":"ai"));
		path=AssetDatabase.GenerateUniqueAssetPath(path+"/"+t.ToString ()+"."+ext);
		AssetDatabase.CreateAsset (so,path);
		AssetDatabase.SaveAssets ();
		return path;
	}
	public static object MakeSimpleDefaultEditor(object ob,System.Type t,string fname,object par) {
		string name=FriendlyString (fname);
		if(t==typeof(int))
			return EditorGUILayout.IntField(name,(int)ob);
		else if(t==typeof(float))
			return EditorGUILayout.FloatField(name,(float)ob);
		else if(t==typeof(bool))
			return EditorGUILayout.Toggle(name,(bool)ob);
		else if(t==typeof(string))
			return EditorGUILayout.TextField(name,(string)ob);
		else if(t==typeof(AnimationCurve))
			return EditorGUILayout.CurveField(name,(AnimationCurve)ob);
		else if(t==typeof(Vector3))
			return EditorGUILayout.Vector3Field(name,(Vector3)ob);
		else if(t==typeof(Vector2))
			return EditorGUILayout.Vector2Field(name,(Vector2)ob);
		else if(t==typeof(Vector4))
			return EditorGUILayout.Vector4Field(name,(Vector4)ob);
		else if(t==typeof(UnityEngine.GameObject)) 
			return EditorGUILayout.ObjectField(name,(Object)ob,t,true);
		else if(t.IsSubclassOf(typeof(UnityEngine.Component))) {
			FieldInfo fedit=null;
			if(editorClass!=null) {
				fedit=editorClass.GetField(t.ToString().ToLower()+"Edit");
			}
			if(fedit==null)
				return EditorGUILayout.ObjectField(name,(Object)ob,t,true);
			else {
				List<ComponentEdit> a=fedit.GetValue (null) as List<ComponentEdit>;
				ComponentEdit ce=null;
				for(int i=0;i<a.Count;i++) if(a[i].com==ob) {ce=a[i];break;}
				if(ce==null) {ce=new ComponentEdit();a.Add (ce);}
				EditorGUILayout.LabelField (name);
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginHorizontal ();
				ob=EditorGUILayout.ObjectField((Object)ob,t,true,GUILayout.Width (150));
				if(GUILayout.Button("New",GUILayout.Width (50))) {
					GameObject to=Selection.activeGameObject;
					GameObject g=GameObject.Find (t+"s");
					if(g==null) g=new GameObject(t+"s");
					GameObject go=new GameObject(to.name+"-"+par.GetType ()+"-"+t);
					go.transform.parent=g.transform;
					go.transform.position=to.transform.position;
					ob=go.AddComponent(t);
				}
				if(GUILayout.Button("Sel",GUILayout.Width (50))) {
					Selection.activeGameObject=((Component)ob).gameObject;
				}
				if(GUILayout.Button((ce.show?"De":"")+"Show",GUILayout.Width (70))) {
					ce.show=!ce.show;
					EditorUtility.SetDirty(ob as Object);
				}
				if(GUILayout.Button((ce.edit?"De":"")+"Edit",GUILayout.Width (70)))
					ce.edit=!ce.edit;
				EditorGUILayout.EndHorizontal ();
				EditorGUI.indentLevel--;
				return (ce.com=ob as Component);
			}
		}
		else if(t==typeof(LayerMask))
			return (LayerMask)(1<<EditorGUILayout.LayerField(name,((LayerMask)ob).value));
		else if(t.IsArray) {
			System.Array a=(System.Array)ob;
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField (name,GUILayout.Width (100));
			System.Type et=t.GetElementType ();
			FieldInfo ft=typeof(Util).GetField ("type"+et.ToString (),BindingFlags.Public|BindingFlags.Static);
			FieldInfo fit=typeof(Util).GetField ("it"+et.ToString (),BindingFlags.NonPublic|BindingFlags.Static);
			List<string> st=null;int it=-1;
			if(et.IsAbstract) {
				st=ft.GetValue (null) as List<string>;
				it=(int)fit.GetValue(null);
				it=EditorGUILayout.Popup(it,st.ToArray ());
				fit.SetValue(null,it);
			}
			FieldInfo fsoa=null;List<string> soa=null;
			if(typeof(ScriptableObject).IsAssignableFrom(et)) {
				fsoa=par.GetType ().GetField (name+"Asset",BindingFlags.Instance|BindingFlags.Public|BindingFlags.IgnoreCase);
				soa=fsoa.GetValue (par) as List<string>;
			}
			if(GUILayout.Button ("New",GUILayout.Width (50))) {
				System.Array ta=System.Array.CreateInstance(et,a.Length+1);
				for(int i=0;i<a.Length;i++) ta.SetValue(a.GetValue(i),i);
				object tob=null;System.Type eet=et;
				if(et.IsAbstract)
					eet=typeof(Util).Assembly.GetType (st[it]);
				if(typeof(ScriptableObject).IsAssignableFrom(eet)) {
					string sos=SOInstance(eet);
					tob=AssetDatabase.LoadAllAssetsAtPath (sos)[0];
					soa.Add (sos);
				}
				else
					tob=System.Activator.CreateInstance (eet);
				ta.SetValue(tob,a.Length);
				return ta;
			}
			if(GUILayout.Button ("Clear",GUILayout.Width (50))) {
				System.Array ta=System.Array.CreateInstance(et,0);
				if(soa!=null) {
					foreach(string s in soa) {
						AssetDatabase.DeleteAsset(s);
					}
					soa.Clear ();
				}
				return ta;
			}
			EditorGUILayout.EndHorizontal ();
			EditorGUI.indentLevel++;
			int oi=-1;
			foreach(object tob in a) {
				oi++;
				string ename=tob.GetType ().ToString();
				FieldInfo no=tob.GetType ().GetField ("name",BindingFlags.Instance|BindingFlags.Public);
				if(no!=null) ename=(string)no.GetValue (tob);
				string efname=ename;
				ename=FriendlyString (ename);
				bool simple=(et.IsValueType||et==typeof(string)||et==typeof(UnityEngine.GameObject)||
					et==typeof(UnityEngine.Component));
				bool bfo=true;
				EditorGUILayout.BeginHorizontal ();
				if(simple) {
					object tob1=MakeSimpleDefaultEditor(tob,et,efname,ob);
					if(tob1!=null||!tob1.Equals(tob)) {
						if(et.IsSubclassOf (typeof(Object))) Undo.RegisterUndo ((Object)ob,ename);
						a.SetValue (tob1,oi);
					}
				}
				else {
					string sename=ename;
					if(typeof(ScriptableObject).IsAssignableFrom(et)) {
						string ssoa=soa[oi];
						ssoa=ssoa.Substring(ssoa.LastIndexOf ("/")+1);
						sename+="-"+ssoa;
					}
					FieldInfo fo=tob.GetType ().GetField ("foldout",BindingFlags.Instance|BindingFlags.Public);
					if(fo!=null) {
						bfo=(bool)fo.GetValue (tob);
						bfo=EditorGUILayout.Foldout(bfo,sename);
						fo.SetValue (tob,bfo);
					} else EditorGUILayout.LabelField(sename);
				}
				if(GUILayout.Button ("X",GUILayout.Width (30))) {
					System.Array ta=System.Array.CreateInstance(et,a.Length-1);
					for(int i=0,j=0;i<a.Length;i++) if(a.GetValue(i)!=tob) ta.SetValue(a.GetValue(i),j++);
					if(soa!=null) {
						AssetDatabase.DeleteAsset(soa[oi]);
						soa.RemoveAt (oi);
					}
					return ta;
				}
				EditorGUILayout.EndHorizontal ();
				if(!simple) {
					EditorGUI.indentLevel++;
					if(bfo) MakeDefaultEditor (tob);
					EditorGUI.indentLevel--;
				}
			}
			EditorGUI.indentLevel--;
		}
		return ob;
	}
	
	public static void  SaveCompAssets(ScriptableObject ob) {
		FieldInfo[] fi=ob.GetType ().GetFields ();
		FieldInfo fa=ob.GetType ().GetField ("compAssets");
		List<string> ca=fa.GetValue (ob) as List<string>;
		ca.Clear ();
		foreach(FieldInfo f in fi) {
			if(!f.FieldType.IsSubclassOf (typeof(Component))) continue;
			Component c=f.GetValue (ob) as Component;
			if(c==null) {ca.Add ("");continue;}
			string s=c.gameObject.name+"/"+f.FieldType;
			Transform t=c.transform;
			while((t=t.parent)!=null) s=t.gameObject.name+"/"+s;
			ca.Add (s);
		}
	}
	public static void LoadCompAssets(ScriptableObject ob) {
		FieldInfo[] fi=ob.GetType ().GetFields ();
		FieldInfo fa=ob.GetType ().GetField ("compAssets");
		List<string> ca=fa.GetValue (ob) as List<string>;
		int i=-1;
		foreach(FieldInfo f in fi) {
			if(!f.FieldType.IsSubclassOf (typeof(Component))) continue;
			i++;
			string s=ca[i];
			if(s.Length==0) continue;
			string sc=s.Substring (s.LastIndexOf ("/")+1);
			s=s.Substring (0,s.LastIndexOf ("/"));
			GameObject g=GameObject.Find (s);
			f.SetValue (ob,g.GetComponent (sc));
		}
	}
	
	
	public static Vector3 VectorClamp(Vector3 v,Vector3 min,Vector3 max) {
		v.x=Mathf.Clamp (v.x,min.x,max.x);
		v.y=Mathf.Clamp (v.y,min.y,max.y);
		v.z=Mathf.Clamp (v.z,min.z,max.z);
		return v;
	}
	
	public static bool IsSaneNumber(float f) {
		if (System.Single.IsNaN(f)) return false;
		if (f==Mathf.Infinity) return false;
		if (f==Mathf.NegativeInfinity) return false;
		if (f>1000000000000) return false;
		if (f<-1000000000000) return false;
		return true;
	}
	
	public static Vector3 Clamp(Vector3 v, float length) {
		float l = v.magnitude;
		if (l > length) return v / l * length;
		return v;
	}
	
	public static float Mod(float x, float period) {
		float r = x % period;
		return (r>=0?r:r+period);
	}
	public static int Mod(int x, int period) {
		int r = x % period;
		return (r>=0?r:r+period);
	}
	public static float Mod(float x) { return Mod(x, 1); }
	public static int Mod(int x) { return Mod(x, 1); }
	
	public static float CyclicDiff(float high, float low, float period, bool skipWrap) {
		if (!skipWrap) {
			high = Mod(high,period);
			low = Mod(low,period);
		}
		return ( high>=low ? high-low : high+period-low );
	}
	public static int CyclicDiff(int high, int low, int period, bool skipWrap) {
		if (!skipWrap) {
			high = Mod(high,period);
			low = Mod(low,period);
		}
		return ( high>=low ? high-low : high+period-low );
	}
	public static float CyclicDiff(float high, float low, float period) { return CyclicDiff(high, low, period, false); }
	public static int CyclicDiff(int high, int low, int period) { return CyclicDiff(high, low, period, false); }
	public static float CyclicDiff(float high, float low) { return CyclicDiff(high, low, 1, false); }
	public static int CyclicDiff(int high, int low) { return CyclicDiff(high, low, 1, false); }
	
	// Returns true is compared is lower than comparedTo relative to reference,
	// which is assumed not to lie between compared and comparedTo.
	public static bool CyclicIsLower(float compared, float comparedTo, float reference, float period) {
		compared = Mod(compared,period);
		comparedTo = Mod(comparedTo,period);
		if (
			CyclicDiff(compared,reference,period,true)
			<
			CyclicDiff(comparedTo,reference,period,true)
		) return true;
		return false;
	}
	public static bool CyclicIsLower(int compared, int comparedTo, int reference, int period) {
		compared = Mod(compared,period);
		comparedTo = Mod(comparedTo,period);
		if (
			CyclicDiff(compared,reference,period,true)
			<
			CyclicDiff(comparedTo,reference,period,true)
		) return true;
		return false;
	}
	public static bool CyclicIsLower(float compared, float comparedTo, float reference) {
		return CyclicIsLower(compared, comparedTo, reference, 1); }
	public static bool CyclicIsLower(int compared, int comparedTo, int reference) {
		return CyclicIsLower(compared, comparedTo, reference, 1); }
	
	public static float CyclicLerp(float a, float b, float t, float period) {
		if (Mathf.Abs(b-a)<=period/2) { return a*(1-t)+b*t; }
		if (b<a) a -= period; else b -= period;
		return Util.Mod(a*(1-t)+b*t);
	}
	
	public static Vector3 ProjectOntoPlane(Vector3 v, Vector3 normal) {
		return v-Vector3.Project(v,normal);
	}
	
	public static Vector3 SetHeight(Vector3 originalVector, Vector3 referenceHeightVector, Vector3 upVector) {
		Vector3 originalOnPlane = ProjectOntoPlane(originalVector, upVector);
		Vector3 referenceOnAxis = Vector3.Project(referenceHeightVector, upVector);
		return originalOnPlane + referenceOnAxis;
	}
	
	public static Vector3 GetHighest(Vector3 a, Vector3 b, Vector3 upVector) {
		if (Vector3.Dot(a,upVector) >= Vector3.Dot(b,upVector)) return a;
		return b;
	}
	public static Vector3 GetLowest(Vector3 a, Vector3 b, Vector3 upVector) {
		if (Vector3.Dot(a,upVector) <= Vector3.Dot(b,upVector)) return a;
		return b;
	}
	
	public static Matrix4x4 RelativeMatrix(Transform t, Transform relativeTo) {
		return relativeTo.worldToLocalMatrix * t.localToWorldMatrix;
	}
	

}

