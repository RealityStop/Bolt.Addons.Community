namespace Unity.VisualScripting.Community 
{
    public class TryFinallySurroundCommand : SurroundCommand
    {
        private TryCatch tryCatch = new TryCatch();
        public override Unit SurroundUnit => tryCatch;
    
        public override ControlOutput surroundSource => tryCatch.@try;
    
        public override ControlOutput surroundExit => sequenceUnit.multiOutputs[1];
    
        public override ControlInput unitEnterPort => tryCatch.enter;
    
        public override IUnitPort autoConnectPort => tryCatch.@finally;
    
        public override string DisplayName => "Try Finally";
    
        public override bool SequenceExit => true;
    } 
}
