import axios from '@/lib/axios-extension';
import url from '@/lib/url-normalize'
export function Pagination(pagination = {}) {
    return axios.get('/api/identity/users?' + url.normalize(pagination));
}
export function GetRoles(id) {
    return axios.get(`/api/identity/users/${id}/roles`)
}
export function SetRoles(id, roles = []) {
    return axios.put(`/api/identity/users/${id}/roles`, roles);
}
export function Get(id) {
    return axios.get(`/api/identity/users/${id}`);
}
export function Update(id, data) {
    return axios.put(`/api/identity/users/${id}`, data);
}
export function Add(data) {
    return axios.post("/api/identity/users", data);
}
export function Delete(id) {
    return axios.delete(`/api/identity/users/${id}`);
}