/*物料信息维护接口*/
import axios from '@/lib/axios-extension';
import url from '@/lib/url-normalize';
export function GetPage(input) {
    return axios.get('/api/wms/material-info/page?' + url.normalize(input));
}
export function Get(id) {
    return axios.get(`/api/wms/material-info/${id}`)
}
export function Create(input) {
    return axios.post("/api/wms/material-info/create", input);
}
export function Update(sku, input) {
    return axios.put(`/api/wms/material-info/${sku}`, input);
}
export function Remove(sku) {
    return axios.delete(`/api/wms/material-info/${sku}`);
}