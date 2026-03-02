#if UNITY_EDITOR
using UnityEditor;

namespace UnityFramework.Editor
{
    // Base type for editor undo commands used by UXTools migrated logic.
    public abstract class UXUndoCommand
    {
        public abstract void Execute();
    }

    public class LocationLineCommand : UXUndoCommand
    {
        private readonly LocationLinesData _linesData;
        private readonly string _undoName;

        public LocationLineCommand(LocationLinesData linesData, string operationName)
        {
            _linesData = linesData;
            _undoName = operationName;
        }

        public override void Execute()
        {
            Undo.IncrementCurrentGroup();
            Undo.RecordObject(_linesData, _undoName);
        }
    }
}
#endif
