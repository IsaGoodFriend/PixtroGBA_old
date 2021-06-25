using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixtro.Compiler {

	public delegate int GetInteger(string _value);
	public delegate float GetFloat(string _value);
	public enum MetaOperations {
		Add, Subtract, Divide, Multiply, Module,
		OpenParenth, CloseParenth,
		Equals, Greater, GreaterEquals, Less, LessEquals,
	}

	public class DataParser {
		public static IEnumerator<string> MetaSplit(string _value) {

			int i;

			string retval = "";

			for (i = 0; i < _value.Length;) {
				while (_value[i] == ' ')
					++i;

				if (_value[i] == '{') {
					i++;
					do {
						if (_value[i] != ' ')
							retval += _value[i];
						i++;
					}
					while (i < _value.Length && _value[i] != '}');

					yield return retval;
					retval = "";
				}
				else {
					do {
						retval += _value[i++];
					}
					while (i < _value.Length && _value[i] != ' ' && _value[i] != '{');

					yield return retval;
					retval = "";
				}
			}

			yield break;
		}
		public static float MetaMath(float a, float b, MetaOperations op) {

			switch (op) {
				case MetaOperations.Add:
					return a + b;
				case MetaOperations.Subtract:
					return a - b;
				case MetaOperations.Multiply:
					return a * b;
				case MetaOperations.Divide:
					return a / b;
				case MetaOperations.Module:
					return a % b;
				default:
					throw new Exception();
			}
		}

		private List<object> operations;
		private int index;
		private bool reset = true;

		public DataParser(string _data) {

			operations = new List<object>();

			var checkEach = MetaSplit(_data);

			while (checkEach.MoveNext()) {
				var parsed = checkEach.Current;


				switch (parsed) {

					case "+":
						operations.Add(MetaOperations.Add);
						break;
					case "-":
						operations.Add(MetaOperations.Subtract);
						break;
					case "*":
						operations.Add(MetaOperations.Multiply);
						break;
					case "/":
						operations.Add(MetaOperations.Divide);
						break;
					case "%":
						operations.Add(MetaOperations.Module);
						break;

					case "(":
						operations.Add(MetaOperations.OpenParenth);
						break;
					case ")":
						operations.Add(MetaOperations.CloseParenth);
						break;

					case ">":
						operations.Add(MetaOperations.Greater);
						break;
					case "=":
					case "==":
						operations.Add(MetaOperations.Equals);
						break;
					case "<":
						operations.Add(MetaOperations.Less);
						break;
					case ">=":
						operations.Add(MetaOperations.GreaterEquals);
						break;
					case "<=":
						operations.Add(MetaOperations.LessEquals);
						break;

					default: 
						operations.Add(parsed);
						break;
				}

			}
			
		}

		public bool GetBoolean(GetFloat _get) {

			reset = true;

			float left = GetFloat(_get);

			reset = false;

			var op = (MetaOperations)operations[index];

			index++;

			float right = GetFloat(_get);

			switch (op) {
				case MetaOperations.Equals:
					return left == right;
				case MetaOperations.Less:
					return left < right;
				case MetaOperations.LessEquals:
					return left <= right;
				case MetaOperations.Greater:
					return left > right;
				case MetaOperations.GreaterEquals:
					return left >= right;
			}


			return false;
		}
		public int GetInteger(GetInteger _get) {

			return 0;
		}
		public float GetFloat(GetFloat _get) {

			if (reset)
				index = 0;

			List<float> stack = new List<float>();
			List<MetaOperations> opStack = new List<MetaOperations>();

			float current = 0;
			MetaOperations currentOp = MetaOperations.Add;

			for (; index < operations.Count; ++index) {


				if (operations[index] is string) {
					float temp;

					if (!float.TryParse(operations[index] as string, out temp)) {
						temp = _get(operations[index] as string);
					}

					current = MetaMath(current, temp, currentOp);
				}
				else if (operations[index] is MetaOperations) {
					MetaOperations op = (MetaOperations)operations[index];

					switch (op) {
						case MetaOperations.Add:
						case MetaOperations.Subtract:
						case MetaOperations.Multiply:
						case MetaOperations.Divide:
						case MetaOperations.Module:
							currentOp = op;
							break;
						case MetaOperations.OpenParenth:
							opStack.Add(currentOp);
							stack.Add(current);
							currentOp = MetaOperations.Add;
							current = 0;
							break;
						case MetaOperations.CloseParenth:
							currentOp = opStack[opStack.Count - 1];
							current = MetaMath(stack[stack.Count - 1], current, currentOp);

							opStack.RemoveAt(opStack.Count - 1);
							stack.RemoveAt(stack.Count - 1);
							break;
						case MetaOperations.Equals:
						case MetaOperations.Less:
						case MetaOperations.Greater:
						case MetaOperations.GreaterEquals:
						case MetaOperations.LessEquals:
							return current;
							
					}
				}
			}

			return current;
		}
	}
}
