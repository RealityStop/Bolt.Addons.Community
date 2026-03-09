namespace Unity.VisualScripting.Community 
{
    public class TryCatchSurroundCommand : SurroundCommand
    {
        private TryCatch tryCatch = new TryCatch();
        public override Unit SurroundUnit => tryCatch;
    
        public override ControlOutput surroundSource => tryCatch.@try;
    
        public override ControlOutput surroundExit => sequenceUnit.multiOutputs[1];
    
        public override ControlInput unitEnterPort => tryCatch.enter;
    
        public override IUnitPort autoConnectPort => tryCatch.@catch;
    
        public override string DisplayName => "Try Catch";
    
        public override bool SequenceExit => true;
    } 
}
