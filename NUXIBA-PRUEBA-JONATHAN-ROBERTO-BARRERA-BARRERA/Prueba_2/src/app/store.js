import { configureStore } from '@reduxjs/toolkit';
import usersReducer from '../features/users/usersSlice';
import contentReducer from '../features/content/contentSlice';
export const store = configureStore({ reducer: { users: usersReducer, content: contentReducer } });
