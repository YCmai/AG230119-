<template>
  <a-modal :visible="true" :width="620" title="物料信息" class="default-hb-modal">
    <a-form-model
      :model="materialForm"
      ref="materialForm"
      class="ant-form-lable-100"
      layout="inline"
      :rules="rules"
    >
      <a-form-model-item label="SKU编码" prop="sku">
        <a-input v-model="materialForm.sku" :disabled="isEdit" />
      </a-form-model-item>
      <a-form-model-item label="物料名称" prop="name">
        <a-input v-model="materialForm.name" />
      </a-form-model-item>
      <a-form-model-item label="物料尺寸" prop="sizeMess">
        <a-input v-model="materialForm.sizeMess" />
      </a-form-model-item>
      <a-form-model-item label="是否容器">
        <a-checkbox v-model="materialForm.isContainer"></a-checkbox>
      </a-form-model-item>
      <a-form-model-item label="物料描述" class="ant-form-item-100">
        <a-textarea v-model="materialForm.describtion"></a-textarea>
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
import { Create, Get, Update } from "@/api/materials/material";
export default {
  name: "MaterialForm",
  data() {
    return {
      loading: false,
      materialForm: {
        sku: "",
        name: "",
        sizeMess: "",
        isContainer: false,
        describtion: "",
      },
      rules: {
        sku: [{ required: true, message: "sku编码不能为空!", trigger: "blur" }],
        name: [{ required: true, message: "物料名称不能为空!", trigger: "blur" }],
      },
      isEdit: false,
    };
  },
  methods: {
    load: async function (sku) {
      this.loading = true;
      try {
        this.isEdit = true;
        let material = await Get(sku);
        this.materialForm = Object.assign(this.materialForm, material);
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
        if (!this.isEdit) await Create(this.materialForm);
        else await Update(this.materialForm.sku, this.materialForm);
        this.$message.success("提交成功!");
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
    valid: function () {
      return new Promise((resolve) => {
        this.$refs.materialForm.validate((valid) => {
          resolve(valid);
        });
      });
    },
  },
};
</script>
