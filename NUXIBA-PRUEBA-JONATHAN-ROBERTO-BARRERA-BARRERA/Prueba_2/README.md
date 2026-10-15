# Prueba 2 — ReactJS, Hooks y Redux

**Autor:** Jonathan Roberto Barrera Barrera

Aplicación React con Vite, Hooks y Redux Toolkit que consume JSONPlaceholder. Carga los primeros 10 usuarios en Redux; permite seleccionar un usuario, consultar publicaciones con sus comentarios y ver tareas ordenadas por `id` descendente. Incluye formulario para enviar tareas mediante `POST /todos`.

## Requisitos y ejecución

- Node.js 18 o posterior y npm.
- Desde esta carpeta:

```bash
npm install
npm run dev
```

Abre la URL local que muestre Vite. Para validar compilación: `npm run build`.

## Funcionalidad

- Usuarios: `GET https://jsonplaceholder.typicode.com/users` (se muestran los primeros 10).
- Publicaciones: `GET /users/{userId}/posts`.
- Comentarios: `GET /posts/{postId}/comments` para cada publicación.
- Tareas: `GET /users/{userId}/todos`, ordenadas por `id` de mayor a menor.
- Crear tarea: `POST https://jsonplaceholder.typicode.com/todos` con `{ "userId": 1, "title": "Ejemplo", "completed": false }`.

JSONPlaceholder simula la creación y devuelve un objeto con un identificador (habitualmente `201`); no persiste el registro en el servidor. La aplicación agrega la respuesta al estado local para mostrar el resultado durante la sesión.

## Arquitectura

- `src/app/store.js`: configuración del store global.
- `src/features/users/usersSlice.js`: estado y carga de usuarios.
- `src/features/content/contentSlice.js`: acciones asíncronas para posts, comentarios y todos.
- `src/App.jsx`: interfaz y composición de componentes.
- `src/styles.css`: estilos adaptables.

## Notas

Las peticiones requieren conexión a internet. Los errores de red se muestran en la interfaz. No se incluye una base de datos propia porque la prueba solicita consumir JSONPlaceholder.
