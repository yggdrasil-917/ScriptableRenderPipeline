using System;

namespace UnityEditor.Rendering.HighDefinition
{
	/// <summary>
	/// Tells a CustomPassDrawer which CustomPass class is intended for the GUI inside the CustomPassDrawer class
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class CustomPassDrawerAttribute : Attribute
	{
		internal Type targetPassType;

		/// <summary>
		/// Indicate that the class is a custom pass drawer and that it replace the default custom pass GUI.
		/// </summary>
		/// <param name="targetPassType">Your Custom Pass type</param>
		public CustomPassDrawerAttribute(Type targetPassType) => this.targetPassType = targetPassType;
	}
}