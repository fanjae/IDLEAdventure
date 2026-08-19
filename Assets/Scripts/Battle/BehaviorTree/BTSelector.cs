using System.Collections.Generic;

public class BTSelector : BTNode
{
    private readonly List<BTNode> children;

    public BTSelector(params BTNode[] children)
    {
        this.children = new List<BTNode>(children);
    }
    public override BTStatus Run()
    {
        for (int i = 0; i < children.Count; i++)
        {
            BTStatus result = children[i].Run();
            if (result != BTStatus.Failure) return result;
        }
        return BTStatus.Failure;
    }
}
