namespace FozruciCS.Utils{
	public class DropOutStack<T>{
		private readonly T[] items;
		private int top;
		public DropOutStack(int capacity)=>items = new T[capacity];

		public void Push(T item){
			items[top++] = item;
			top %= items.Length;
		}
		public T Pop(){
			top = ((items.Length + top) - 1) % items.Length;
			return items[top];
		}

		public T Peek(int i = -1){
			i = i == -1 ? top : i;
			return items[i];
		}
	}
}
