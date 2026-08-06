using System;
using System.Collections.Generic;

[Serializable]
public class PlayerInventoryData
{
    // Lista de IDs de los objetos que el jugador tiene en su inventario general
    public List<string> ownedItems = new List<string>();

    // Equipamiento actual (Múltiples espadas al mismo tiempo)
    public List<string> equippedWeapons = new List<string>();
    
    public string equippedArmorId = "";
    
    // Si tienes mascotas u otros equipables, se pueden agregar aquí
    public string equippedPetId = "";
}
