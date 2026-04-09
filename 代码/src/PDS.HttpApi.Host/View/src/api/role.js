import axios from '@/lib/axios-extension';
import url from '@/lib/url-normalize'
export function Pagination(pagination = {}) {
    return axios.get("/api/wms/role/page?" + url.normalize(pagination));
}
export function Get(id) {
    return axios.get(`/api/wms/role/get/${id}`);
}
export function Add(record) {
    return axios.post("/api/wms/role/create", record);

}
export function Update(id, record) {
    return axios.put(`/api/wms/role/update/${id}`, record);
}
export function Delete(id) {
    return axios.delete(`/api/wms/role/delete/${id}`);
}
export function GetAll() {
    return axios.get("/api/identity/roles/all");
}