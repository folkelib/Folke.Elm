namespace Folke.Elm.Visitor
{
    /// <summary>A selected field</summary>
    public class SelectedField : IVisitable
    {
        public Field Field { get; set; }
        
        /// <summary>Gets or sets the index in the results in the sql data reader</summary>
        public int Index { get; set; }

        public void Accept(IVisitor visitor)
        {
            if (Index > 0)
                visitor.DuringFields();
            Field.Accept(visitor);
        }
    }
}