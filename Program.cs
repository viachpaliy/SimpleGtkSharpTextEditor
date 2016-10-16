using System;
using Gtk;


namespace TextEditor
{
	public class Program 
	{
		

		public static void Main()
		{
			Application.Init ();
		   var window = new Window ("TextEditor");
			window.DeleteEvent += OnDelete;
			window.SetDefaultSize (1000, 640);
			VBox vbox = new VBox (false, 1);
			HBox toolbar = new HBox ();
			TextEditor tex=new TextEditor(window);

			toolbar.PackStart(tex.fileToolbar,true,true,0);
			toolbar.PackStart (new SeparatorToolItem (), false, false, 0);
			toolbar.PackStart (tex.editToolbar, true, true, 0);

			vbox.PackStart (toolbar, false, false, 0);
			vbox.PackStart (tex.editorWindow, true, true, 0);

			window.Add (vbox);

			window.ShowAll ();
		
			Application.Run ();
		}

		static void OnDelete (object o, DeleteEventArgs e)
		{
			Application.Quit ();
		}	


	}


}

