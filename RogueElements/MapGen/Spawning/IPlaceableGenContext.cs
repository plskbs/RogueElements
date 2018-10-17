﻿using System;
using System.Collections.Generic;

namespace RogueElements
{
    

    public interface IPlaceableGenContext<T> : IGenContext
    {
        List<Loc> GetAllFreeTiles();
        List<Loc> GetFreeTiles(Rect rect);
        bool CanPlaceItem(Loc loc);
        void PlaceItem(Loc loc, T item);
    }
    
}