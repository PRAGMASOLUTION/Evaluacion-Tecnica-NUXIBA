import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
const API = 'https://jsonplaceholder.typicode.com';
export const fetchUsers = createAsyncThunk('users/fetchUsers', async () => {
  const response = await fetch(`${API}/users`); if (!response.ok) throw new Error('No fue posible cargar usuarios'); return response.json();
});
const usersSlice = createSlice({ name: 'users', initialState: { items: [], status: 'idle', error: null, selectedId: null }, reducers: { selectUser(state, action) { state.selectedId = action.payload; } }, extraReducers: b => b.addCase(fetchUsers.pending, s => { s.status = 'loading'; s.error = null; }).addCase(fetchUsers.fulfilled, (s, a) => { s.status = 'succeeded'; s.items = a.payload.slice(0, 10); }).addCase(fetchUsers.rejected, (s, a) => { s.status = 'failed'; s.error = a.error.message; }) });
export const { selectUser } = usersSlice.actions; export default usersSlice.reducer;
