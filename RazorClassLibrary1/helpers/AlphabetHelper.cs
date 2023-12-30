namespace RazorClassLibrary.helpers {
	public class AlphabetHelper {
		public List<Alphabet>? AlphabetList;
		public void BuildAlphabet() {
			if (AlphabetList == null) {
				AlphabetList = new List<Alphabet>();
				Alphabet alphabet = new Alphabet() { Id = 1, Letter = "A" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 2, Letter = "B" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 3, Letter = "C" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 4, Letter = "D" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 5, Letter = "E" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 6, Letter = "F" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 7, Letter = "G" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 8, Letter = "H" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 9, Letter = "I" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 10, Letter = "J" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 11, Letter = "K" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 12, Letter = "L" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 13, Letter = "M" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 14, Letter = "N" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 15, Letter = "O" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 16, Letter = "P" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 17, Letter = "Q" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 18, Letter = "R" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 19, Letter = "S" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 20, Letter = "T" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 21, Letter = "U" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 22, Letter = "V" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 23, Letter = "W" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 24, Letter = "X" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 25, Letter = "Y" };
				AlphabetList.Add(alphabet);
				alphabet = new Alphabet() { Id = 26, Letter = "Z" };
				AlphabetList.Add(alphabet);
			}
		}
	}
	public class Alphabet {
		public int Id { get; set; }
		public  required string Letter { get; set; }
	}
}
