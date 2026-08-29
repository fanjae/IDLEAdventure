using System.Collections.Generic;
//여러 조건이나 행동을 순서대로 실행하는 노드
public class BTSequence : BTNode
{
    private readonly List<BTNode> children;

    public BTSequence(params BTNode[] children)
    {
        this.children = new List<BTNode>(children);
    }
    public override BTStatus Run()
    {
        for (int i = 0; i < children.Count; i++)
        {
            BTStatus result = children[i].Run();
            if (result != BTStatus.Success) return result;
        }
        return BTStatus.Success;
    }
}
