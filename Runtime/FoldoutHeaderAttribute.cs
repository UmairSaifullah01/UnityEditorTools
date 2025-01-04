using UnityEngine;


namespace THEBADDEST.EditorTools
{


	public class FoldoutHeaderAttribute : PropertyAttribute
	{
		public string Header;
		public bool   Expanded;

		public FoldoutHeaderAttribute(string header)
		{
			Header   = header;
			Expanded = true; // Default state
		}
	}


}