using System.Collections.Generic;
using UnityEngine;

public class QuizLevel : MonoBehaviour
{
    [Header("Configuración del Nivel")]
    [Tooltip("Nombre del nivel (ej. Nivel 1, Nivel 2, Nivel 3)")]
    public string levelName = "Nivel 3 - Difícil";

    [Tooltip("Lista de preguntas de este nivel")]
    public List<Question> questions = new List<Question>();

    private void Reset()
    {
        string objName = gameObject.name.ToLower();
        if (objName.Contains("1"))
        {
            levelName = "Nivel 1 - Bajo";
            LoadLevel1Questions();
        }
        else if (objName.Contains("2"))
        {
            levelName = "Nivel 2 - Intermedio";
            LoadLevel2Questions();
        }
        else if (objName.Contains("3"))
        {
            levelName = "Nivel 3 - Difícil";
            LoadLevel3Questions();
        }
    }

    [ContextMenu("Cargar las 12 Preguntas en Nivel 1")]
    public void LoadLevel1Questions()
    {
        levelName = "Nivel 1 - Bajo";
        questions = new List<Question>
        {
            new Question(
                "1. ¿Qué es Unity?",
                new string[4] { "A) Un motor de videojuegos", "B) Un lenguaje de modelado 3D", "C) Una tarjeta gráfica", "D) Un programa para editar vídeos" },
                0,
                "Unity es un motor de desarrollo de videojuegos en tiempo real utilizado para crear experiencias 2D y 3D."
            ),
            new Question(
                "2. ¿Para qué sirve Blender?",
                new string[4] { "A) Para programar redes", "B) Para crear modelos 3D y animaciones", "C) Para publicar videojuegos automáticamente", "D) Para gestionar servidores" },
                1,
                "Blender es un software de código abierto para modelado 3D, animación, renderizado y esculpido."
            ),
            new Question(
                "3. ¿Qué es un GameObject en Unity?",
                new string[4] { "A) Un objeto de la escena", "B) Un tipo de ordenador", "C) Una tienda de videojuegos", "D) Un archivo de audio exclusivamente" },
                0,
                "Un GameObject es la entidad básica que representa cualquier elemento dentro de una escena en Unity."
            ),
            new Question(
                "4. ¿Qué es una escena en Unity?",
                new string[4] { "A) Una lista de contraseñas", "B) Un nivel o espacio donde organizamos objetos", "C) Un modelo 3D obligatorio", "D) Un tipo de textura" },
                1,
                "Las escenas son los niveles, menús o espacios donde se estructuran y organizan todos los GameObjects."
            ),
            new Question(
                "5. ¿Qué es un material en Unity?",
                new string[4] { "A) Un objeto que guarda las partidas", "B) Un elemento que define cómo se ve una superficie", "C) Un nivel completo", "D) Un archivo de texto" },
                1,
                "Un material define el aspecto visual de una superficie combinando texturas, colores y propiedades del shader."
            ),
            new Question(
                "6. ¿Para qué sirve una textura?",
                new string[4] { "A) Para añadir detalles visuales a una superficie", "B) Para aumentar automáticamente los FPS", "C) Para crear código C#", "D) Para guardar una escena" },
                0,
                "Una textura es una imagen 2D que se proyecta sobre la superficie de un objeto 3D para darle color y detalle."
            ),
            new Question(
                "7. ¿Qué lenguaje se utiliza habitualmente para programar en Unity?",
                new string[4] { "A) HTML", "B) C#", "C) SQL", "D) CSS" },
                1,
                "C# (C Sharp) es el lenguaje de programación estándar y oficial utilizado en Unity."
            ),
            new Question(
                "8. ¿Qué hace la cámara de un videojuego?",
                new string[4] { "A) Dibuja los modelos 3D", "B) Muestra la vista del juego", "C) Compila el código", "D) Crea las texturas" },
                1,
                "La cámara captura y renderiza el mundo del juego para mostrárselo al jugador en la pantalla."
            ),
            new Question(
                "9. ¿Qué significa FPS en videojuegos?",
                new string[4] { "A) Frames Per Second (fotogramas por segundo)", "B) Fast Player System", "C) Final Project Scene", "D) First Program Started" },
                0,
                "FPS son los fotogramas mostrados por segundo; determinan la fluidez visual del juego."
            ),
            new Question(
                "10. ¿Qué conviene hacer mientras desarrollas un juego?",
                new string[4] { "A) No probarlo hasta terminarlo", "B) Probarlo frecuentemente", "C) Borrar todas las versiones anteriores", "D) Crear todos los gráficos antes de tener una idea" },
                1,
                "Probar y testear iterativamente permite detectar fallos y ajustar la jugabilidad temprano."
            ),
            new Question(
                "11. ¿Qué es un bug?",
                new string[4] { "A) Un error o fallo del programa", "B) Un tipo de personaje", "C) Un motor gráfico", "D) Un efecto de sonido" },
                0,
                "Un bug es un defecto, error o comportamiento inesperado en el software o videojuego."
            ),
            new Question(
                "12. ¿Cuál es la mejor forma de empezar a crear tu primer videojuego?",
                new string[4] { "A) Crear un MMO enorme", "B) Empezar por un proyecto pequeño", "C) Comprar el ordenador más caro", "D) Programar sin probar nada" },
                1,
                "Iniciar con proyectos reducidos asegura terminar juegos y consolidar el aprendizaje paso a paso."
            )
        };
    }

    [ContextMenu("Cargar las 12 Preguntas en Nivel 2")]
    public void LoadLevel2Questions()
    {
        levelName = "Nivel 2 - Intermedio";
        questions = new List<Question>
        {
            new Question(
                "1. ¿Qué es un Prefab en Unity?",
                new string[4] { "A) Una copia de seguridad del proyecto", "B) Una plantilla reutilizable de un GameObject", "C) Una textura para modelos 3D", "D) Un tipo de escena" },
                1,
                "Un Prefab permite guardar y reutilizar objetos con sus componentes y configuración."
            ),
            new Question(
                "2. ¿Para qué sirve el componente Rigidbody?",
                new string[4] { "A) Para aplicar física a un objeto", "B) Para cambiar el color de un objeto", "C) Para controlar la cámara", "D) Para importar modelos 3D" },
                0,
                "Rigidbody permite que un objeto participe en la simulación física de Unity (gravedad, colisiones dinámicas, etc.)."
            ),
            new Question(
                "3. ¿Qué diferencia hay entre Update() y FixedUpdate()?",
                new string[4] { "A) No existe ninguna diferencia", "B) Update se usa para físicas y FixedUpdate para interfaz", "C) Update se ejecuta por fotograma y FixedUpdate sigue el paso fijo de física", "D) FixedUpdate solo funciona en 2D" },
                2,
                "Update se llama cada fotograma (variable); FixedUpdate se ejecuta en intervalos regulares y fijos de física."
            ),
            new Question(
                "4. ¿Qué es un Collider en Unity?",
                new string[4] { "A) Un sistema de iluminación", "B) Un componente que define una forma para detectar colisiones", "C) Un formato de audio", "D) Una herramienta para crear animaciones" },
                1,
                "Los Colliders definen volúmenes que permiten detectar contactos y otras interacciones físicas."
            ),
            new Question(
                "5. ¿Para qué sirve el UV Mapping en Blender?",
                new string[4] { "A) Para reducir automáticamente los polígonos", "B) Para asignar una textura 2D a la superficie de un modelo 3D", "C) Para crear luces", "D) Para exportar scripts" },
                1,
                "El UV Mapping relaciona puntos de la superficie 3D con coordenadas en una imagen 2D plana."
            ),
            new Question(
                "6. ¿Qué ocurre al aumentar demasiado el número de polígonos de un modelo?",
                new string[4] { "A) Siempre mejora el rendimiento", "B) El modelo deja de necesitar texturas", "C) Puede aumentar el coste de renderizado y afectar al rendimiento", "D) Unity los elimina siempre al importar" },
                2,
                "Más geometría incrementa el coste de procesamiento gráfico en la GPU y puede causar caídas de fotogramas."
            ),
            new Question(
                "7. ¿Qué es un shader?",
                new string[4] { "A) Un programa que determina cómo se representa visualmente una superficie", "B) Un archivo de guardado", "C) Una herramienta de audio", "D) Una copia de una escena" },
                0,
                "Los shaders calculan aspectos visuales como color, iluminación, sombras, transparencia y reflejos."
            ),
            new Question(
                "8. ¿Para qué se utiliza el Bake de iluminación?",
                new string[4] { "A) Para guardar el proyecto en la nube", "B) Para precalcular parte de la iluminación y reutilizar sus resultados", "C) Para comprimir scripts C#", "D) Para animar personajes automáticamente" },
                1,
                "La iluminación horneada (Lightmap) precalcula sombras y luces estáticas, ahorrando rendimiento en tiempo real."
            ),
            new Question(
                "9. ¿Qué es un Array en programación?",
                new string[4] { "A) Una colección de elementos del mismo tipo, con tamaño definido al crearse", "B) Una función que destruye objetos", "C) Un tipo de material", "D) Una cámara secundaria" },
                0,
                "En C#, un array almacena múltiples elementos del mismo tipo organizados en índices con tamaño fijo."
            ),
            new Question(
                "10. ¿Qué función tiene Git durante el desarrollo de un videojuego?",
                new string[4] { "A) Aumentar los FPS", "B) Crear modelos 3D", "C) Controlar versiones y registrar cambios en el proyecto", "D) Generar música automáticamente" },
                2,
                "Git permite guardar versiones del código y assets, comparar cambios y colaborar en equipo con seguridad."
            ),
            new Question(
                "11. ¿Qué significa que una variable sea privada (private) en C#?",
                new string[4] { "A) Que solo puede utilizarse desde cualquier clase", "B) Que no puede cambiar nunca", "C) Que su acceso directo está limitado principalmente a la clase que la declara", "D) Que Unity la elimina al iniciar" },
                2,
                "private encapsula el dato y restringe su acceso directo desde fuera de la clase donde fue declarada."
            ),
            new Question(
                "12. ¿Qué es un sistema LOD en videojuegos 3D?",
                new string[4] { "A) Un sistema que cambia el nivel de detalle según la distancia o condiciones de visualización", "B) Un formato de sonido", "C) Una técnica para crear interfaces", "D) Un sistema de guardado de partidas" },
                0,
                "Level of Detail intercambia mallas por versiones más simplificadas cuando la cámara está lejos para ahorrar recursos."
            )
        };
    }

    [ContextMenu("Cargar las 12 Preguntas en Nivel 3")]
    public void LoadLevel3Questions()
    {
        levelName = "Nivel 3 - Difícil";
        questions = new List<Question>
        {
            new Question(
                "1. Si eliminas el componente Mesh Renderer de un cubo 3D, pero mantienes su Mesh Filter, ¿qué ocurre normalmente?",
                new string[4] {
                    "A) El cubo deja de existir en la escena.",
                    "B) El cubo sigue teniendo su geometría, pero deja de renderizarse normalmente.",
                    "C) Unity elimina automáticamente su Transform.",
                    "D) El cubo se convierte automáticamente en un objeto 2D."
                },
                1, // Correcta: B
                "El Mesh Filter contiene la referencia a la malla; el Mesh Renderer se encarga de representarla visualmente."
            ),
            new Question(
                "2. Un objeto hijo tiene posición local (0, 0, 2) y su padre se desplaza 5 en X. Sin rotación ni escala, ¿cuál es su posición mundial?",
                new string[4] {
                    "A) El hijo mantiene exactamente la misma posición mundial.",
                    "B) El hijo pasa a tener una posición mundial (5, 0, 2).",
                    "C) El hijo pasa a (0, 0, 7).",
                    "D) Unity elimina su posición local."
                },
                1, // Correcta: B
                "La posición local se define respecto al padre. Al desplazarse el padre, también cambia la posición mundial del hijo."
            ),
            new Question(
                "3. Instancias un Prefab y cambias su color en Inspector. Luego modificas el Prefab original. ¿Qué afirmación es correcta?",
                new string[4] {
                    "A) Las modificaciones siempre se transfieren automáticamente al Prefab original.",
                    "B) La instancia hereda cambios, pero las modificaciones locales se mantienen como overrides.",
                    "C) Modificar un Prefab nunca afecta a sus instancias.",
                    "D) El Prefab original se destruye al crear una instancia."
                },
                1, // Correcta: B
                "Unity gestiona las modificaciones de instancias mediante overrides, conservando los cambios locales específicos."
            ),
            new Question(
                "4. Un componente está habilitado, pero su GameObject inicia desactivado. Al activarse, ¿cuál es el orden habitual de ejecución?",
                new string[4] {
                    "A) Start() -> Awake() -> OnEnable()",
                    "B) OnEnable() -> Start() -> Awake()",
                    "C) Awake() -> OnEnable() -> Start()",
                    "D) Start() -> OnEnable() -> Awake()"
                },
                2, // Correcta: C
                "En un objeto inactivo al cargar, Awake() se ejecuta primero al activarse, luego OnEnable() y antes del primer Update(), Start()."
            ),
            new Question(
                "5. Dos cubos tienen Box Collider pero ninguno tiene Rigidbody. ¿Qué describe mejor su comportamiento?",
                new string[4] {
                    "A) Siempre rebotan al tocarse.",
                    "B) Unity los trata como colliders estáticos; no se simulan dinámicamente con gravedad.",
                    "C) Se convierten automáticamente en triggers.",
                    "D) No pueden participar en ningún tipo de detección de colisiones."
                },
                1, // Correcta: B
                "Los colliders sin Rigidbody suelen considerarse geometría estática; para simular movimiento dinámico se requiere Rigidbody."
            ),
            new Question(
                "6. Un objeto tiene Box Collider con Is Trigger activo. Otro objeto atraviesa su volumen. ¿Qué comportamiento se espera?",
                new string[4] {
                    "A) El objeto queda bloqueado como ante una pared sólida.",
                    "B) Se produce un evento como OnTriggerEnter, sin actuar como superficie sólida que impida el paso.",
                    "C) Unity destruye ambos objetos automáticamente.",
                    "D) El trigger solo funciona si el objeto es visible por la cámara."
                },
                1, // Correcta: B
                "Los triggers detectan entradas y salidas de volúmenes espaciales sin bloquear físicamente el movimiento."
            ),
            new Question(
                "7. Quieres aplicar fuerzas mediante Rigidbody.AddForce() a un objeto físico. ¿Dónde es más apropiado ejecutar esa lógica?",
                new string[4] {
                    "A) En FixedUpdate(), teniendo en cuenta el modo de fuerza utilizado.",
                    "B) Exclusivamente en OnGUI().",
                    "C) En Awake(), una sola vez, para controlar todo el movimiento futuro.",
                    "D) En LateUpdate(), porque siempre se ejecuta a intervalos fijos."
                },
                0, // Correcta: A
                "FixedUpdate() se ejecuta con intervalos fijos y constantes, siendo el lugar estándar para aplicar fuerzas físicas."
            ),
            new Question(
                "8. Para mantener un tamaño visual parejo de la interfaz en distintas resoluciones, ¿qué ajuste suele usarse en el Canvas?",
                new string[4] {
                    "A) Canvas Scaler en Constant Pixel Size sin ningún otro ajuste.",
                    "B) Canvas Scaler en Scale With Screen Size, con resolución de referencia y ajuste apropiado.",
                    "C) Eliminar el componente Rect Transform de todos los elementos.",
                    "D) Convertir el Canvas en un objeto 3D con Rigidbody."
                },
                1, // Correcta: B
                "Scale With Screen Size adapta la escala de la UI respecto a una resolución de referencia dada (ej. 1920x1080)."
            ),
            new Question(
                "9. Un objeto está en la escena, pero no aparece en la vista de juego (Game View). ¿Cuál de estas causas es posible?",
                new string[4] {
                    "A) Su capa está excluida de Culling Mask de la cámara.",
                    "B) El objeto tiene un nombre que contiene números.",
                    "C) Su Transform tiene una posición local distinta de cero.",
                    "D) El objeto tiene un Box Collider."
                },
                0, // Correcta: A
                "La cámara solo renderiza los objetos cuyas capas (Layers) estén marcadas y habilitadas en su Culling Mask."
            ),
            new Question(
                "10. Varios objetos usan el mismo material. ¿Qué riesgo hay al cambiar el color de un material compartido directamente en el asset?",
                new string[4] {
                    "A) El color solo cambia en el objeto seleccionado y nunca en los demás.",
                    "B) La modificación afecta a todos los objetos que usan el material y altera la configuración guardada.",
                    "C) Unity convierte automáticamente el material en una textura.",
                    "D) El material deja de funcionar con cualquier luz."
                },
                1, // Correcta: B
                "Un material compartido afecta a todos los renderers que lo referencian. Para cambios individuales se usa una instancia o MaterialPropertyBlock."
            ),
            new Question(
                "11. Un objeto mantiene una referencia directa a un GameObject de otra escena que es descargada. ¿Qué problema ocurre?",
                new string[4] {
                    "A) La referencia se convierte automáticamente en un Prefab permanente.",
                    "B) El objeto referenciado deja de estar disponible y la referencia queda nula o destruida.",
                    "C) Unity carga siempre de nuevo la escena sin solicitarlo.",
                    "D) La escena descargada sigue consumiendo exactamente los mismos recursos."
                },
                1, // Correcta: B
                "Descargar una escena destruye sus objetos en memoria; las referencias directas hacia ellos quedan nulas (MissingReference)."
            ),
            new Question(
                "12. Un muro opaco tapa objetos que están dentro del campo visual de la cámara. ¿Qué técnica evita renderizarlos?",
                new string[4] {
                    "A) Occlusion Culling, si la escena y configuración permiten detectar esa oclusión.",
                    "B) Frustum Culling exclusivamente, porque descarta objetos tapados por paredes.",
                    "C) Texture Filtering, porque elimina objetos invisibles.",
                    "D) VSync, porque impide renderizar geometría oculta."
                },
                0, // Correcta: A
                "Frustum Culling descarta fuera de cámara; Occlusion Culling descarta objetos dentro del campo de visión que quedan ocultos tras objetos opacos."
            )
        };
    }
}
