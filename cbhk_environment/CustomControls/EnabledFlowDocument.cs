using System.Windows.Documents;

namespace cbhk_environment.CustomControls
{
    internal class EnabledFlowDocument:FlowDocument
    {
        protected override bool IsEnabledCore
        {
            get { return true; }
        }
    }
}
