namespace TinyCompiler
{
    public class Emitter
    {
        private string _filePath;
        private string _header;
        private string _code;

        public Emitter(string filePath)
        {
            _filePath = filePath;
        }

        public void Emit(string code)
        {
            _code += code;
        }

        public void EmitLine(string code)
        {
            _code += code + Environment.NewLine;
        }

        public void HeaderLine(string code)
        {
            _header += code + Environment.NewLine;
        }

        public void WriteFile()
        {
            File.WriteAllText(_filePath, _header + _code);
        }
    }
}
