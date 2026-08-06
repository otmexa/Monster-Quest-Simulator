using UnityEngine;
using System.IO;

public class DataManager : MonoBehaviour
{
    // Implementación del patrón Singleton para fácil acceso global
    public static DataManager Instance { get; private set; }

    // Esta es la variable principal a la que todo el juego accederá
    public PlayerData Data { get; private set; }

    private string saveFilePath;

    private void Awake()
    {
        // Nos aseguramos de que solo exista un DataManager en todo el juego
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Evita que se destruya al cambiar de escena
            
            // Definimos la ruta segura donde se guardará el JSON
            saveFilePath = Path.Combine(Application.persistentDataPath, "PlayerData.json");
            
            // Cargamos los datos apenas arranca el juego
            LoadData();
        }
        else
        {
            Destroy(gameObject); // Si ya hay uno (ej: al volver a la escena del Menú Principal), destruimos el duplicado
        }
    }

    // Llama a esto cada vez que el jugador compre algo, gane XP, o cambie un ajuste
    public void SaveData()
    {
        // Convertimos nuestra clase PlayerData a texto JSON (el 'true' lo hace bonito y legible)
        string json = JsonUtility.ToJson(Data, true);
        
        // Escribimos el archivo en el disco duro local
        File.WriteAllText(saveFilePath, json);
        
        Debug.Log("Datos guardados exitosamente en: " + saveFilePath);
    }

    // Llama a esto para forzar una recarga (usualmente solo se llama en Awake)
    public void LoadData()
    {
        if (File.Exists(saveFilePath))
        {
            // Si el archivo existe, lo leemos y reconstruimos la clase PlayerData
            string json = File.ReadAllText(saveFilePath);
            Data = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log("Datos cargados correctamente.");
        }
        else
        {
            // Si es la primera vez que abre el juego, creamos datos nuevos y limpios
            Debug.Log("No se encontró archivo de guardado. Creando un perfil nuevo...");
            Data = new PlayerData();
            SaveData(); // Guardamos este perfil nuevo inmediatamente
        }
    }
}
