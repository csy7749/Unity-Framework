using UnityEngine;

namespace GameLogic
{
    public static class RaycastHelper
    {
        public static bool CastMapPoint(out Vector3 hitPoint)
        {
            if (Camera.main != null)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500, 1 << LayerMask.NameToLayer("Map")))
                {
                    hitPoint = hit.point;
                    return true;
                }
            }

            hitPoint = Vector3.zero;
            return false;
        }

        public static bool CastUnitObj(out GameObject castObj)
        {
            if (Camera.main != null)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500, (1 << LayerMask.NameToLayer("Enemy")) | 1 << LayerMask.NameToLayer("Hero")))
                {
                    castObj = hit.collider.gameObject;
                    return true;
                }
            }

            castObj = null;
            return false;
        }

        public static bool CastEnemyObj(out GameObject castObj)
        {
            if (Camera.main != null)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500, 1 << LayerMask.NameToLayer("Enemy")))
                {
                    castObj = hit.collider.gameObject;
                    return true;
                }
            }

            castObj = null;
            return false;
        }

        public static bool CastHeroObj(out GameObject castObj)
        {
            if (Camera.main != null)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500, 1 << LayerMask.NameToLayer("Hero")))
                {
                    castObj = hit.collider.gameObject;
                    return true;
                }
            }

            castObj = null;
            return false;
        }
    }
}