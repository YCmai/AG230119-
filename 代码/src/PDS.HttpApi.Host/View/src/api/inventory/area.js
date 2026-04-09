import axios from '@/lib/axios-extension';
import url from '@/lib/url-normalize';
export function CreateArea(input) {
    return axios.post('/api/wms/warehouse/area/create', input);
}
export function UpdateArea(wareHouseId, areaCode, input) {
    return axios.put(`/api/wms/warehouse/area/${wareHouseId}/${areaCode}`, input);
}
export function DeleteArea(wareHouseId, areaCode) {
    return axios.delete(`/api/wms/warehouse/area/${wareHouseId}/${areaCode}`);
}
export function GetArea(wareHouseId, areaCode) {
    return axios.get(`/api/wms/warehouse/area/${wareHouseId}/${areaCode}`)
}