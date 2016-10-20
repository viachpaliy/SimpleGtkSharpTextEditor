using System;
using Gtk;
using System.IO;
using System.Collections.Generic;
using Pango;

namespace TextEditor
{
	public class TextEditor
	{
		public Toolbar  fileToolbar;
		public Toolbar editToolbar;
		public ScrolledWindow editorWindow;
		public string fileName;
		public MenuItem file;
		Menu filemenu;
		public MenuItem edit;
		Menu editmenu;
		Window parent;
		bool isSaved;
		Stack<string> undoStack;
		Stack<string> redoStack;
		string clipboard;

		TextView editorView;

		MenuItem newfile;
		MenuItem open;
		MenuItem save;
		MenuItem saveAs;
		MenuItem close;

		MenuItem copy;
		MenuItem paste;
		MenuItem cut;
		MenuItem undo;
		MenuItem redo;

		ToolButton newBtn;
		ToolButton openBtn;
		ToolButton saveBtn;
		ToolButton saveAsBtn;
		ToolButton closeBtn;


		ToolButton copyBtn;
		ToolButton pasteBtn;
		ToolButton cutBtn;
		ToolButton undoBtn;
		ToolButton redoBtn;

		public TextEditor (Window parent)
		{
			this.parent = parent;
			editorWindow = new ScrolledWindow ();
			editorView = new TextView ();
			editorWindow.Add (editorView);

			file = new MenuItem ("File");
			filemenu = new Menu ();
			file.Submenu = filemenu;
			newfile = new MenuItem ("New file");
			open = new MenuItem ("Open file");
			save = new MenuItem ("Save file");
			saveAs = new MenuItem ("Save file as ...");
			close = new MenuItem ("Close");
			filemenu.Append (newfile);
			filemenu.Append (open);
			filemenu.Append (save);
			filemenu.Append (saveAs);
			filemenu.Append (close);

			edit = new MenuItem ("Edit");
			editmenu = new Menu ();
			edit.Submenu = editmenu;
			copy = new MenuItem ("Copy");
			paste = new MenuItem ("Paste");
			cut = new MenuItem ("Cut");
			undo = new MenuItem ("Undo");
			redo = new MenuItem ("Redo");
			editmenu.Append (copy);
			editmenu.Append (paste);
			editmenu.Append (cut);
			editmenu.Append (undo);
			editmenu.Append (redo);

			editToolbar = new Toolbar ();
			copyBtn = new ToolButton (Stock.Copy);
			copyBtn.TooltipText = "Copy";
			pasteBtn = new ToolButton (Stock.Paste);
			pasteBtn.TooltipText = "Paste";
			cutBtn = new ToolButton (Stock.Cut);
			cutBtn.TooltipText = "Cut";
			undoBtn = new ToolButton (Stock.Undo);
			undoBtn.TooltipText = "Undo";
			redoBtn = new ToolButton (Stock.Redo);
			redoBtn.TooltipText = "Redo";
			editToolbar.Insert (copyBtn, 0);
			editToolbar.Insert (pasteBtn, 1);
			editToolbar.Insert (cutBtn, 2);
			editToolbar.Insert (undoBtn, 3);
			editToolbar.Insert (redoBtn, 4);

			 undoStack = new Stack<string> ();
			 redoStack = new Stack<string> ();
		


			fileToolbar = new Toolbar ();
			newBtn = new ToolButton (Stock.New);
			newBtn.TooltipText = "New";
			openBtn = new ToolButton (Stock.Open);
			openBtn.TooltipText = "Open";
			saveBtn = new ToolButton (Stock.Save);
			saveBtn.TooltipText = "Save";
			saveAsBtn = new ToolButton (Stock.SaveAs);
			saveAsBtn.TooltipText = "Save as...";
			closeBtn = new ToolButton (Stock.Close);
			closeBtn.TooltipText = "Close";

			fileToolbar.Insert (newBtn, 0);
			fileToolbar.Insert (openBtn, 1);
			fileToolbar.Insert (saveBtn, 2);
			fileToolbar.Insert (saveAsBtn, 3);
			fileToolbar.Insert (closeBtn, 4);






			editorView.Buffer.Clear ();

			fileName = Directory.GetCurrentDirectory () + "/Untitled";
			parent.Title = fileName;
			isSaved = true;
			saveBtn.Sensitive = false;
			save.Sensitive = false;
			undoBtn.Sensitive = false;
			undo.Sensitive = false;
			undoStack.Clear ();
			redoBtn.Sensitive = false;
			redo.Sensitive = false;
			redoStack.Clear ();

			pasteBtn.Sensitive = false;
			paste.Sensitive = false;

			editorView.Buffer.Changed += OnTextChanged;
			editorView.Buffer.UserActionBegun += OnUserActionBegun;


			newfile.Activated += OnNew;
			newBtn.Clicked += OnNew ;
			open.Activated += OnOpen;
			openBtn.Clicked += OnOpen;
			save.Activated += OnSave;
			saveBtn.Clicked += OnSave;
			saveAs.Activated += OnSaveAs;
			saveAsBtn.Clicked += OnSaveAs;
			close.Activated += OnClose;
			closeBtn.Clicked += OnClose;
			undo.Activated += OnUndo;
			undoBtn.Clicked += OnUndo;
			redo.Activated += OnRedo;
			redoBtn.Clicked += OnRedo;
			copy.Activated += OnCopy;
			copyBtn.Clicked += OnCopy;
			cut.Activated += OnCut;
			cutBtn.Clicked += OnCut;
			paste.Activated += OnPaste;
			pasteBtn.Clicked += OnPaste;
		}

	


		void OnClose(object sender, EventArgs args)
		{
			bool cancel = false;
			if (!isSaved)
			{
				var dialog = new Dialog ();
				dialog.AddButton ("Close without save", ResponseType.No);
				dialog.AddButton (Stock.Cancel, ResponseType.Cancel);
				dialog.AddButton ("Save changes", ResponseType.Ok);
				var dialogLabel = new Label ("Save changes in file?");
				dialog.VBox.PackStart (dialogLabel, false, false, 0);
				dialog.ShowAll ();
				int response = dialog.Run ();
				if (response == (int)ResponseType.Ok) {
					System.IO.File.WriteAllText (fileName, editorView.Buffer.Text);
				}
				if (response == (int)ResponseType.Cancel) {
					cancel = true;}
				dialog.Destroy ();
			}
			if (!cancel) {
				Application.Quit ();
			}
		}

		void OnPaste(object sender, EventArgs args)
		{			
			editorView.Buffer.InsertAtCursor (clipboard);
		}

		void OnCut(object sender, EventArgs args)
		{
			TextIter startIter;
			TextIter finishIter;
			if (editorView.Buffer.GetSelectionBounds (out startIter, out finishIter)) {
				clipboard = editorView.Buffer.GetText (startIter, finishIter, true);
				if(pasteBtn.Sensitive==false)pasteBtn.Sensitive = true;
				if (paste.Sensitive == false) paste.Sensitive = true; 
				undoStack.Push (editorView.Buffer.Text);
				if (undoBtn.Sensitive == false)	undoBtn.Sensitive = true;
				if (undo.Sensitive == false) undo.Sensitive = true;
				editorView.Buffer.Delete (ref startIter, ref finishIter);
			}

		}

		void OnCopy(object sender, EventArgs args)
		{
			TextIter startIter;
			TextIter finishIter;
			if (editorView.Buffer.GetSelectionBounds (out startIter, out finishIter)) {
				clipboard = editorView.Buffer.GetText (startIter, finishIter, true);
				if(pasteBtn.Sensitive==false)pasteBtn.Sensitive = true;
				if (paste.Sensitive == false) paste.Sensitive = true; 
			}
		}

		void OnRedo(object sender, EventArgs args)
		{
			undoStack.Push (editorView.Buffer.Text);
			if (undoBtn.Sensitive == false)	undoBtn.Sensitive = true;
			if (undo.Sensitive == false) undo.Sensitive = true;
			if (redoStack.Count>0)	editorView.Buffer.Text = redoStack.Pop ();
			if (redoStack.Count == 0) {
				redoBtn.Sensitive = false;
				redo.Sensitive = false;
			}
				
		}

		void OnUndo(object sender, EventArgs args)
		{
			redoStack.Push (editorView.Buffer.Text);
			if (redoBtn.Sensitive == false)	redoBtn.Sensitive = true;
			if (redo.Sensitive == false)	redo.Sensitive = true;
			if (undoStack.Count>0)	editorView.Buffer.Text = undoStack.Pop ();
			if (undoStack.Count == 0) {
				undoBtn.Sensitive = false;
				undo.Sensitive = false;
			}
		}

		void OnUserActionBegun(object sender, EventArgs args)
		{
			undoStack.Push (editorView.Buffer.Text);
			if (undoBtn.Sensitive == false)	undoBtn.Sensitive = true;
			if (undo.Sensitive == false) undo.Sensitive = true;
		}

		void OnTextChanged(object sender, EventArgs args)
		{
			isSaved = false;
			if (saveBtn.Sensitive==false) saveBtn.Sensitive = true;
			if (save.Sensitive==false) save.Sensitive = true;
		}

		void OnNew(object sender, EventArgs args)
		{
			bool cancel = false;
			if (!isSaved)
			{
				var dialog = new Dialog ();
				dialog.AddButton ("Close without save", ResponseType.No);
				dialog.AddButton (Stock.Cancel, ResponseType.Cancel);
				dialog.AddButton ("Save changes", ResponseType.Ok);
				var dialogLabel = new Label ("Save changes in file?");
				dialog.VBox.PackStart (dialogLabel, false, false, 0);
				dialog.ShowAll ();
				int response = dialog.Run ();
				if (response == (int)ResponseType.Ok) {
					System.IO.File.WriteAllText (fileName, editorView.Buffer.Text);
					}
				if (response == (int)ResponseType.Cancel) {
					cancel = true;}
				dialog.Destroy ();
			}
			if (!cancel) {
				editorView.Buffer.Clear ();
				fileName = Directory.GetCurrentDirectory () + "/Untitled";
				parent.Title = fileName;
				isSaved = true;
				saveBtn.Sensitive = false;
				save.Sensitive = false;
				undoBtn.Sensitive = false;
				undo.Sensitive = false;
				undoStack.Clear ();
				redoBtn.Sensitive = false;
				redo.Sensitive = false;
				redoStack.Clear ();
			}
		}

		void OnSave(object sender, EventArgs args)
		{
			System.IO.File.WriteAllText (fileName, editorView.Buffer.Text);
			isSaved = true;
			saveBtn.Sensitive = false;
			save.Sensitive = false;

		}

		void OnSaveAs(object sender, EventArgs args)
		{
			
			Gtk.FileChooserDialog filechooser =
				new Gtk.FileChooserDialog ("Save file",
					parent,
					FileChooserAction.Save);
			filechooser.AddButton(Stock.Cancel, ResponseType.Cancel);
			filechooser.AddButton(Stock.Save, ResponseType.Ok);

			if (filechooser.Run () == (int)ResponseType.Ok) {
				System.IO.File.WriteAllText (filechooser.Filename, editorView.Buffer.Text);
				fileName=filechooser.Filename;
				parent.Title = fileName;
				isSaved = true;
				saveBtn.Sensitive = false;
				save.Sensitive = false;
			}

			filechooser.Destroy();
		

		}

	 void OnOpen (object sender, EventArgs args)
		{
			bool cancel = false;
			if (!isSaved) {
				var dialog = new Dialog ();
				dialog.AddButton ("Close without save", ResponseType.No);
				dialog.AddButton (Stock.Cancel, ResponseType.Cancel);
				dialog.AddButton ("Save changes", ResponseType.Ok);
				var dialogLabel = new Label ("Save changes in file?");
				dialog.VBox.PackStart (dialogLabel, false, false, 0);
				dialog.ShowAll ();
				int response = dialog.Run ();
				if (response == (int)ResponseType.Ok) {
					System.IO.File.WriteAllText (fileName, editorView.Buffer.Text);
				}
				if (response == (int)ResponseType.Cancel) {
					cancel = true;
				}
				dialog.Destroy ();
			}

			if (!cancel) {
			
				Gtk.FileChooserDialog filechooser =
					new Gtk.FileChooserDialog ("Choose the file to open",
						parent,
						FileChooserAction.Open
					);
				filechooser.Visible = true;
				filechooser.AddButton (Stock.Cancel, ResponseType.Cancel);
				filechooser.AddButton (Stock.Open, ResponseType.Ok);

				if (filechooser.Run () == (int)ResponseType.Ok) {

					fileName = filechooser.Filename;
					editorView.Buffer.Text = System.IO.File.ReadAllText (filechooser.Filename);

				}

				filechooser.Destroy ();
				parent.Title = fileName;
				isSaved = true;
				saveBtn.Sensitive = false;
				save.Sensitive = false;
				undoBtn.Sensitive = false;
				undo.Sensitive = false;
				undoStack.Clear ();
				redoBtn.Sensitive = false;
				redo.Sensitive = false;
				redoStack.Clear ();
			}
	
		}

	}
}


