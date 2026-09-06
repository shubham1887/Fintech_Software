using FintechApp.Entities;

namespace FintechApp.Helpers
{
    /// <summary>
    /// Network-hierarchy helpers. The user tree is defined by <see cref="UserMaster.ParentID"/>;
    /// these walk it to compute a caller's full downline (all descendants, however deep).
    /// </summary>
    public static class UserHierarchy
    {
        /// <summary>
        /// Every descendant of <paramref name="rootUserId"/> (children, grandchildren, …). The root
        /// itself is NOT included, so a caller never appears in their own downline. Cycle-safe.
        /// </summary>
        public static HashSet<int> GetDownlineIds(IEnumerable<UserMaster> allUsers, int rootUserId)
        {
            var childrenByParent = allUsers
                .Where(u => u.ParentID.HasValue)
                .GroupBy(u => u.ParentID!.Value)
                .ToDictionary(g => g.Key, g => g.Select(u => u.UserMasterID).ToList());

            var result = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(rootUserId);
            while (queue.Count > 0)
            {
                var parentId = queue.Dequeue();
                if (!childrenByParent.TryGetValue(parentId, out var children)) continue;
                foreach (var childId in children)
                    if (result.Add(childId)) queue.Enqueue(childId);
            }
            return result;
        }
    }
}
