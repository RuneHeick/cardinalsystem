using MatrixSystem.ObjectMatrix.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MatrixSystem
{

    delegate void PositionChangedHandler(ItemContainer obj, PositionValue position);

    public delegate void ItemZoneChange(GameObject gameObject, PositionValue position, ZoneTypes currentZone, ZoneTypes oldZone);

    class ItemContainer
    {
        public event PositionChangedHandler OnLocationChanged;

        private GameObject gameObject;
        public GameObject GameObject
        {
            get
            {
                return gameObject; 
            }
            set
            {
                if (gameObject != null)
                    gameObject.GameObjectDataChanged -= location_check;
                gameObject = value;
                if (gameObject != null)
                    gameObject.GameObjectDataChanged += location_check;
            }
        } 

        public int CurrentAreaX { get; set; }
        public int CurrentAreaY { get; set; }

        private void location_check(GameObject game, UInt64 changedArray)
        {
            if ((changedArray & (1 << (int)PublicSyncTypes.POSITION)) != 0)
            {
                PositionValue position = game.Position;
                if (position != null)
                {
                    OnLocationChanged?.Invoke(this, position);
                }
            }
        }

        public void Dispose()
        {
            if (gameObject != null)
                gameObject.GameObjectDataChanged -= location_check;
        }
    }

    class LocationChangedContainer
    {
        public ItemContainer obj { get; set; }
        public PositionValue position { get; set; }
    }

    public class GridController
    {
        public const double sectorSize = 1000000f;
        public const int size = 100;

        private List<LocationChangedContainer> locationChanges = new List<LocationChangedContainer>(2000);
        private List<ItemContainer>[ , ] list = new List<ItemContainer>[size+2, size+2];

        public event ItemZoneChange OnItemZoneChanged;

        private void AreaContainer_OnLocationChanged(ItemContainer obj, PositionValue position)
        {
            locationChanges.Add(new LocationChangedContainer() { obj = obj, position = position });
        }

        public GridController()
        {
            
            for (int x = 0; x < size + 2; x++)
            {
                for (int y = 0; y < size + 2; y++)
                {
                    list[x, y] = new List<ItemContainer>();
                }
            }
        }

        public void UpdateMatrix()
        {
            locationChanges.Clear();

            for (int x = 1; x < size+1; x++)
            {
                for (int y = 1; y < size+1; y++)
                {
                    //Update matrix with the side areas. 
                    List<ItemContainer> l = list[x - 1, y - 1];
                    IEnumerable<ItemContainer> items = l;
                    l = list[x - 1, y];
                    items = Enumerable.Concat(items, l);
                    l = list[x - 1, y + 1];
                    items = Enumerable.Concat(items, l);

                    l = list[x, y - 1];
                    items = Enumerable.Concat(items, l);

                    List<ItemContainer> fieldE = list[x, y];
                    items = Enumerable.Concat(items, fieldE);

                    l = list[x, y + 1];
                    items = Enumerable.Concat(items, l);

                    l = list[x + 1, y - 1];
                    items = Enumerable.Concat(items, l);
                    l = list[x + 1, y];
                    items = Enumerable.Concat(items, l);
                    l = list[x + 1, y + 1];
                    items = Enumerable.Concat(items, l);

                    foreach (var a in fieldE)
                    {
                        foreach (var b in items)
                        {
                            if(a.GameObject != b.GameObject)
                                b.GameObject.AffectObject(a.GameObject);
                        }
                    }

                    //update the field in the center
                    foreach (var a in fieldE)
                    {
                        a.GameObject.UpdateObject();
                    }
                }
            }

            // Do move of items that needs moving. 
            foreach (var moveItems in locationChanges)
            {
                int x = (int)(moveItems.position.X / (sectorSize / size)) + 1;
                int y = (int)(moveItems.position.Y / (sectorSize / size)) + 1;
                var currentList = list[moveItems.obj.CurrentAreaX, moveItems.obj.CurrentAreaY];

                ZoneTypes currentZone = GetZone(x, y);
                ZoneTypes oldZone = GetZone(moveItems.obj.CurrentAreaX, moveItems.obj.CurrentAreaY);

                if (currentZone == ZoneTypes.OUT_OF_AREA)
                {
                    OnItemZoneChanged?.Invoke(moveItems.obj.GameObject, moveItems.position, ZoneTypes.OUT_OF_AREA, oldZone);
                    currentList.Remove(moveItems.obj);
                    RemoveItem(moveItems.obj);
                }
                else
                {
                    var l = list[x, y];
                    if (currentList != l)
                    {
                        if (currentZone != oldZone)
                            OnItemZoneChanged?.Invoke(moveItems.obj.GameObject, moveItems.position, currentZone, oldZone);

                        currentList.Remove(moveItems.obj);
                        l.Add(moveItems.obj);
                        moveItems.obj.CurrentAreaX = x;
                        moveItems.obj.CurrentAreaY = y;
                    }
                }
            }

        }

        public static ZoneTypes GetZone(int x, int y)
        {
            if (x == 0 || y == 0 || x == size + 1 || y == size + 1)
                return ZoneTypes.IN_TRANSFERE_ZONE;
            else if (x > size + 1 || y > size + 1 || x < 0 || y < 0)
                return ZoneTypes.OUT_OF_AREA;
            else if (x == 1 && y == 1)
                return ZoneTypes.IN_SYNC_ZONE_LOWER_LEFT;
            else if (x == 1 && y < size)
                return ZoneTypes.IN_SYNC_ZONE_MID_LEFT;
            else if (x == 1 && y == size)
                return ZoneTypes.IN_SYNC_ZONE_UPPER_LEFT;
            else if (x == size && y == 0)
                return ZoneTypes.IN_SYNC_ZONE_LOWER_RIGHT;
            else if (x == size && y < size)
                return ZoneTypes.IN_SYNC_ZONE_MID_RIGHT;
            else if (x == size && y == size)
                return ZoneTypes.IN_SYNC_ZONE_UPPER_RIGHT;
            else if (x > 1 && y == 0)
                return ZoneTypes.IN_SYNC_ZONE_LOWER_MID;
            else if (x > 1 && y == size)
                return ZoneTypes.IN_SYNC_ZONE_UPPER_MID;
            else
                return ZoneTypes.IN_NORMAL_ZONE;
        }

        private void RemoveItem(ItemContainer container)
        {
            container.Dispose();
            container.OnLocationChanged -= AreaContainer_OnLocationChanged;
        }

        public void AddGameObject(GameObject game)
        {
            PositionValue position = game.Position;
            if(position != null)
            {
                int x = (int)(position.X / (sectorSize / size))+1;
                int y = (int)(position.Y / (sectorSize / size))+1;
                List<ItemContainer> l = list[x, y];
                var container = new ItemContainer() { GameObject = game, CurrentAreaX = x, CurrentAreaY = y };
                container.OnLocationChanged += AreaContainer_OnLocationChanged;
                l.Add(container);
                OnItemZoneChanged?.Invoke(game, game.Position, GetZone(x,y), ZoneTypes.OUT_OF_AREA);
            }
        }

        public void GetClose(GameObject game, Action<GameObject> itemAction)
        {
            PositionValue position = game.Position;
            if (position != null)
            {

                int x = (int)(position.X / (sectorSize / size)) + 1;
                int y = (int)(position.Y / (sectorSize / size)) + 1;


                List<ItemContainer> l = list[x, y];
                l.ForEach((i) => { if (game.ObjectID != i.GameObject.ObjectID) { itemAction(i.GameObject); }});

                if (y > 0)
                {
                    l = list[x, y - 1];
                    l.ForEach((i) => itemAction(i.GameObject));
                }

                if (y < size+1)
                {
                    l = list[x, y + 1];
                    l.ForEach((i) => itemAction(i.GameObject));
                }

                if (x > 0 && y > 0)
                {
                    l = list[x - 1, y - 1];
                    l.ForEach((i) => itemAction(i.GameObject));
                }

                if (x > 0)
                {
                    l = list[x - 1, y];
                    l.ForEach((i) => itemAction(i.GameObject));
                }

                if (x > 0 && y < size + 1)
                {
                    l = list[x - 1, y + 1];
                    l.ForEach((i) => itemAction(i.GameObject));
                }

                if (x < size + 1 && y > 0)
                {
                    l = list[x + 1, y - 1];
                    l.ForEach((i) => itemAction(i.GameObject));
                }

                if (x < size + 1)
                {
                    l = list[x + 1, y];
                    l.ForEach((i) => itemAction(i.GameObject));
                }

                if (x < size + 1 && y < size + 1)
                {
                    l = list[x + 1, y + 1];
                    l.ForEach((i) => itemAction(i.GameObject));
                }
            }
        }

    }
}
