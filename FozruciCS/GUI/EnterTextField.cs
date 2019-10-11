using NStack;
using Terminal.Gui;

namespace FozruciCS.GUI{
	public class EnterTextField : TextField{
		public delegate void TextEnterHandler(string text);
		public EnterTextField(string text) : base(text){}

		public override bool ProcessKey(KeyEvent kb){
			if(kb.Key == Key.Enter){
				TextEnter?.Invoke(Text.ToString());
				Text = ustring.Empty;
				Used = false;
			}

			return base.ProcessKey(kb);
		}
		public event TextEnterHandler TextEnter;
	}
}
