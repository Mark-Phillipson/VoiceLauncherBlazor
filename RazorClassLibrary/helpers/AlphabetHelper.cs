namespace RazorClassLibrary.helpers {
	public class AlphabetHelper {
			public List<Alphabet>? AlphabetList;
			private static readonly HashSet<char> _blacklist = new HashSet<char>{'C','D','E','F'};
			public void BuildAlphabet() {
				if (AlphabetList == null) {
					AlphabetList = new List<Alphabet>();
					int id = 1;
					for (char c = 'A'; c <= 'Z'; c++) {
						if (_blacklist.Contains(char.ToUpperInvariant(c))) continue;
						AlphabetList.Add(new Alphabet { Id = id++, Letter = c.ToString() });
					}
				}
			}
			public string? GetLetterByIndex(int index) {
				if (AlphabetList == null) BuildAlphabet();
				return AlphabetList?.FirstOrDefault(a => a.Id == index)?.Letter;
			}
	}
	public class Alphabet {
		public int Id { get; set; }
		public  required string Letter { get; set; }
	}
}
