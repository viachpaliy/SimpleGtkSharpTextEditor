using System;
using Gtk;
using System.IO;
using System.Collections.Generic;

namespace TextEditor
{
	public class TextEditor
	{
		public Toolbar  fileToolbar;
		public Toolbar editToolbar;
		public ScrolledWindow editorWindow;
		public string fileName;
		Window parent;
		bool isSaved;
		Stack<string> undoStack;
		Stack<string> redoStack;
		string clipboard;

		TextView editorView;

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

			editToolbar = new Toolbar ();
			copyBtn = new ToolButton (Stock.Copy);
			pasteBtn = new ToolButton (Stock.Paste);
			cutBtn = new ToolButton (Stock.Cut);
			undoBtn = new ToolButton (Stock.Undo);
			redoBtn = new ToolButton (Stock.Redo);
			editToolbar.Insert (copyBtn, 0);
			editToolbar.Insert (pasteBtn, 1);
			editToolbar.Insert (cutBtn, 2);
			editToolbar.Insert (undoBtn, 3);
			editToolbar.Insert (redoBtn, 4);

			 undoStack = new Stack<string> ();
			 redoStack = new Stack<string> ();
		


			fileToolbar = new Toolbar ();
			newBtn = new ToolButton (Stock.New);
			openBtn = new ToolButton (Stock.Open);
			saveBtn = new ToolButton (Stock.Save);
			saveAsBtn = new ToolButton (Stock.SaveAs);
			closeBtn = new ToolButton (Stock.Close);
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
			undoBtn.Sensitive = false;
			undoStack.Clear ();
			redoBtn.Sensitive = false;
			redoStack.Clear ();

			pasteBtn.Sensitive = false;

			editorView.Buffer.Changed += OnTextChanged;
			editorView.Buffer.UserActionBegun += OnUserActionBegun;

			newBtn.Clicked += OnNew ;
			openBtn.Clicked += OnOpen;
			saveBtn.Clicked += OnSave;
			saveAsBtn.Clicked += OnSaveAs;
			closeBtn.Clicked += OnNew;
			undoBtn.Clicked += OnUndo;
			redoBtn.Clicked += OnRedo;
			copyBtn.Clicked += OnCopy;
			cutBtn.Clicked += OnCut;
			pasteBtn.Clicked += OnPaste;
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
				undoStack.Push (editorView.Buffer.Text);
				if (undoBtn.Sensitive == false)
					undoBtn.Sensitive = true;
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
			}
		}

		void OnRedo(object sender, EventArgs args)
		{
			undoStack.Push (editorView.Buffer.Text);
			if (undoBtn.Sensitive == false)
				undoBtn.Sensitive = true;
			if (redoStack.Count>0)
			editorView.Buffer.Text = redoStack.Pop ();
			if (redoStack.Count == 0)
				redoBtn.Sensitive = false;
		}

		void OnUndo(object sender, EventArgs args)
		{
			redoStack.Push (editorView.Buffer.Text);
			if (redoBtn.Sensitive == false)
				redoBtn.Sensitive = true;
			if (undoStack.Count>0)
			editorView.Buffer.Text = undoStack.Pop ();
			if (undoStack.Count == 0)
				undoBtn.Sensitive = false;
		}

		void OnUserActionBegun(object sender, EventArgs args)
		{
			undoStack.Push (editorView.Buffer.Text);
			if (undoBtn.Sensitive == false)
				undoBtn.Sensitive = true;
		}

		void OnTextChanged(object sender, EventArgs args)
		{
			isSaved = false;
			if (saveBtn.Sensitive==false) saveBtn.Sensitive = true;
		}

		void OnNew(object sender, EventArgs args)
		{
			bool cancel = false;
			if (!isSaved)
			{
				var dialog = new Dialog ();
				dialog.AddButton ("Save without changes", ResponseType.No);
				dialog.AddButton (Stock.Cancel, ResponseType.Cancel);
				dialog.AddButton ("Save changes", ResponseType.Ok);
				var dialogLabel = new Label ("Save changes in file?");
				dialog.VBox.PackStart (dialogLabel, false, false, 0);
				dialog.ShowAll ();
				if (dialog.Run () == (int)ResponseType.Ok) {
					System.IO.File.WriteAllText (fileName, editorView.Buffer.Text);
					}
				if (dialog.Run () == (int)ResponseType.Cancel) {
					cancel = true;}
				dialog.Destroy ();
			}
			if (!cancel) {
				editorView.Buffer.Clear ();
				fileName = Directory.GetCurrentDirectory () + "/Untitled";
				parent.Title = fileName;
				isSaved = true;
				saveBtn.Sensitive = false;
				undoBtn.Sensitive = false;
				undoStack.Clear ();
				redoBtn.Sensitive = false;
				redoStack.Clear ();
			}
		}

		void OnSave(object sender, EventArgs args)
		{
			System.IO.File.WriteAllText (fileName, editorView.Buffer.Text);
			isSaved = true;
			saveBtn.Sensitive = false;

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
				if (dialog.Run () == (int)ResponseType.Ok) {
					System.IO.File.WriteAllText (fileName, editorView.Buffer.Text);
				}
				if (dialog.Run () == (int)ResponseType.Cancel) {
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
				undoBtn.Sensitive = false;
				undoStack.Clear ();
				redoBtn.Sensitive = false;
				redoStack.Clear ();
			}
	
		}

	}
}


