<template>
  <a-modal :visible="true" :width="620" title="储位信息" class="default-hb-modal">
    <a-form-model
      :model="storeForm"
      ref="storeForm"
      layout="inline"
      :rules="rules"
      class="ant-form-lable-100"
    >
      <a-form-model-item label="所属仓库" prop="wareHouseId">
        <a-select
          class="form-model-item-select"
          v-model="storeForm.wareHouseId"
          @change="selectWareHouse"
          placeholder="请选择仓库"
          :disabled="isEdit"
        >
          <a-select-option
            v-for="(ware, index) in wareHouses"
            :key="index"
            :value="ware.id"
          >
            {{ ware.id }}
          </a-select-option>
        </a-select>
      </a-form-model-item>
      <a-form-model-item label="所属区域" prop="wareHouseAreaCode">
        <a-select
          class="form-model-item-select"
          v-model="storeForm.wareHouseAreaCode"
          :disabled="isEdit"
        >
          <a-select-option
            v-for="(area, index) in wareHouseAreas"
            :key="index"
            :value="area.code"
          >
            {{ area.code }}
          </a-select-option>
        </a-select>
      </a-form-model-item>
      <a-form-model-item label="上级储位" prop="parentId">
        <a-select
          :disabled="isEdit"
          class="form-model-item-select"
          show-search
          v-model="storeForm.parentId"
          :default-active-first-option="false"
          :show-arrow="false"
          :filter-option="false"
          :not-found-content="null"
          @search="searchStore"
          @change="selectParent"
          placeholder="输入搜索父级储位"
        >
          <a-select-option v-for="d in stores" :key="d.id">
            {{ d.id }}
          </a-select-option>
        </a-select>
      </a-form-model-item>
      <a-form-model-item label="储位编码" prop="id">
        <a-input v-model="storeForm.id" placeholder="请输入储位编码"></a-input>
      </a-form-model-item>
      <a-form-model-item label="储位名称" prop="name">
        <a-input v-model="storeForm.name" placeholder="请输入储位名称"></a-input>
      </a-form-model-item>
      <a-form-model-item label="地图节点">
        <a-input v-model="storeForm.mapNodeName" placeholder="请输入地图节点"></a-input>
      </a-form-model-item>
      <a-form-model-item label="尺寸信息">
        <a-input v-model="storeForm.sizeMes" placeholder="请输入尺寸信息"></a-input>
      </a-form-model-item>
      <a-form-model-item label="起始高度">
        <a-input-number
          class="form-model-item-input-number"
          v-model="storeForm.startHeight"
          :formatter="(value) => `${value}mm`"
          :parser="(value) => value.replace('mm', '')"
          :min="0"
        >
        </a-input-number>
      </a-form-model-item>
      <a-form-model-item label="储位描述" class="ant-form-item-100">
        <a-textarea></a-textarea>
      </a-form-model-item>
    </a-form-model>
    <template slot="footer">
      <a-button key="back" @click="close" size="small">
        {{ $t("actions.cancel") }}
      </a-button>
      <a-button key="submit" type="primary" :loading="loading" @click="save" size="small">
        {{ $t("actions.save") }}
      </a-button>
    </template>
  </a-modal>
</template>
<script>
import component from "@/lib/base";
import { GetAllWareHouses } from "@/api/inventory/wareHouse";
import { CreateStore, GetStore, QueryStore, UpdateStore } from "@/api/inventory/store";
export default {
  name: "StoreForm",
  mixins: [component],
  data() {
    return {
      loading: false,
      rules: {
        wareHouseId: [{ required: true, message: "必须选择仓库" }],
        wareHouseAreaCode: [{ required: true, message: "必须选择区域" }],
        id: [{ required: true, message: "储位编码不能为空" }],
        // name: [{ required: true, message: "储位名称不能为空" }],
      },
      storeForm: {
        id: "",
        wareHouseId: "",
        wareHouseAreaCode: "",
        mapNodeName: "",
        name: "",
        sizeMes: "",
        startHeight: 0,
        parentId: "",
        parent: null,
      },
      wareHouses: [],
      wareHouseAreas: [],
      stores: [],
      isEdit: false,
    };
  },
  // computed: {
  //   isEdit() {
  //     if (this.storeForm.creationTime) return true;
  //     return false;
  //   },
  // },
  mounted() {
    this.init();
  },
  methods: {
    init: async function (id = null, parentId = null) {
      this.loading = true;
      try {
        this.wareHouses = await GetAllWareHouses();
        if (parentId) {
          let parent = await GetStore(parentId);
          this.setParent(parent);
        }
        if (id) {
          this.isEdit = true;
          let storeForm = await GetStore(id);
          this.storeForm = Object.assign(this.storeForm, storeForm);
          this.setParent(storeForm);
        }
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        this.loading = false;
      }
    },
    save: async function () {
      if (!(await this.valid())) return;
      this.loading = true;
      try {
        if (this.isEdit) await UpdateStore(this.storeForm);
        else await CreateStore(this.storeForm);
        this.$message.success("保存成功!");
        this.close();
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        this.loading = false;
      }
    },
    close: function () {
      this.$emit("close");
    },
    selectWareHouse: function (id) {
      let wareHouse = this.wareHouses.filter((m) => {
        return m.id == id;
      })[0];
      this.wareHouseAreas = wareHouse.areas;
      this.storeForm.wareHouseAreaCode = "";
    },
    valid: function () {
      return new Promise((resolve) => {
        this.$refs.storeForm.validate((valid) => {
          resolve(valid);
        });
      });
    },
    setParent: function (parent) {
      this.storeForm.parent = parent;
      this.storeForm.parentId = parent.id;
      this.storeForm.wareHouseId = parent.wareHouseId;
      this.selectWareHouse(parent.wareHouseId);
      this.storeForm.wareHouseAreaCode = parent.wareHouseIdAreaCode;
    },
    clearParent: function () {
      this.storeForm.parent = null;
      this.storeForm.parentId = "";
      this.storeForm.wareHouseId = "";
      this.storeForm.wareHouseAreaCode = "";
    },
    searchStore: async function (value) {
      try {
        this.stores = await QueryStore(value);
      } catch (err) {
        console.log("查询父级储位失败!");
      }
    },
    selectParent: async function (value) {
      let parent = this.stores.filter((m) => {
        return m.id == value;
      })[0];
      if (parent) this.setParent(parent);
    },
  },
};
</script>
