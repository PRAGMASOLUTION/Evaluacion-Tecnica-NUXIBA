import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
const API = 'https://jsonplaceholder.typicode.com';
export const fetchPosts = createAsyncThunk('content/fetchPosts', async userId => {
  const pRes = await fetch(`${API}/users/${userId}/posts`);
  if (!pRes.ok) throw new Error('No fue posible cargar publicaciones');
  const posts = await pRes.json(); const allComments = await Promise.all(posts.map(async p => { const r = await fetch(`${API}/posts/${p.id}/comments`); if (!r.ok) throw new Error('Error cargando comentarios'); return { postId: p.id, comments: await r.json() }; }));
  return posts.map(post => ({ ...post, comments: allComments.find(x => x.postId === post.id)?.comments ?? [] }));
});
export const fetchTodos = createAsyncThunk('content/fetchTodos', async userId => { const r = await fetch(`${API}/users/${userId}/todos`); if (!r.ok) throw new Error('No fue posible cargar tareas'); return (await r.json()).sort((a,b)=>b.id-a.id); });
export const createTodo = createAsyncThunk('content/createTodo', async ({ userId, title, completed }) => { const r = await fetch(`${API}/todos`, { method: 'POST', headers: { 'Content-Type': 'application/json; charset=UTF-8' }, body: JSON.stringify({ userId, title, completed }) }); if (!r.ok) throw new Error('No fue posible guardar la tarea'); return r.json(); });
const slice = createSlice({ name: 'content', initialState: { view: 'posts', posts: [], todos: [], status: 'idle', error: null, saving: false, saveMessage: '' }, reducers: { setView(s,a){s.view=a.payload; s.error=null; s.saveMessage='';} }, extraReducers: b => {
 b.addCase(fetchPosts.pending,s=>{s.status='loading';s.error=null;}).addCase(fetchPosts.fulfilled,(s,a)=>{s.status='succeeded';s.posts=a.payload;}).addCase(fetchPosts.rejected,(s,a)=>{s.status='failed';s.error=a.error.message;});
 b.addCase(fetchTodos.pending,s=>{s.status='loading';s.error=null;}).addCase(fetchTodos.fulfilled,(s,a)=>{s.status='succeeded';s.todos=a.payload;}).addCase(fetchTodos.rejected,(s,a)=>{s.status='failed';s.error=a.error.message;});
 b.addCase(createTodo.pending,s=>{s.saving=true;s.saveMessage='';s.error=null;}).addCase(createTodo.fulfilled,(s,a)=>{s.saving=false;s.todos.unshift(a.payload);s.todos.sort((x,y)=>y.id-x.id);s.saveMessage=`Solicitud aceptada por JSONPlaceholder (id: ${a.payload.id}). El servicio simula la creación y no persiste los datos.`;}).addCase(createTodo.rejected,(s,a)=>{s.saving=false;s.error=a.error.message;});
}}); export const { setView } = slice.actions; export default slice.reducer;
